'use client';

import { useState } from 'react';
import { useForm, Resolver } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { safeRequest, rawClient } from '@/shared/api/client';
import type {
  MakePqrsdfRequest,
  MakePqrsdfResponse,
  PqrsdfType,
  IdentificationType,
} from '../types/pqrsdf';

const pqrsdfSchema = z
  .object({
    type: z.enum(['Petition', 'Complaint', 'Claim', 'Suggestion', 'Denunciation', 'Compliment']),
    destinationAreaId: z.string().min(1, 'Debe seleccionar el área de destino.'),
    isAnonymous: z.boolean(),
    applicant: z
      .object({
        fullName: z.string().optional(),
        identificationType: z.enum(['CC', 'CE', 'TI', 'PA', 'NIT']).optional(),
        identificationNumber: z.string().optional(),
        email: z.string().optional(),
        phoneNumber: z.string().optional(),
      })
      .optional()
      .nullable(),
    subject: z
      .string()
      .min(5, 'El asunto debe contener al menos 5 caracteres.')
      .max(150, 'El asunto no puede exceder 150 caracteres.'),
    description: z
      .string()
      .min(10, 'La descripción debe contener al menos 10 caracteres.')
      .max(4000, 'La descripción no puede exceder 4000 caracteres.'),
  })
  .superRefine((data, ctx) => {
    if (data.isAnonymous && data.type !== 'Denunciation' && data.type !== 'Suggestion') {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message:
          'La radicación anónima está permitida exclusivamente para Denuncias y Sugerencias.',
        path: ['isAnonymous'],
      });
    }

    if (!data.isAnonymous) {
      if (!data.applicant?.fullName || data.applicant.fullName.trim().length < 3) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'El nombre completo debe tener al menos 3 caracteres.',
          path: ['applicant', 'fullName'],
        });
      }

      if (!data.applicant?.identificationType) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Seleccione el tipo de documento.',
          path: ['applicant', 'identificationType'],
        });
      }

      if (
        !data.applicant?.identificationNumber ||
        data.applicant.identificationNumber.trim().length < 4
      ) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'El número de documento debe tener al menos 4 caracteres.',
          path: ['applicant', 'identificationNumber'],
        });
      }

      if (!data.applicant?.email || !data.applicant.email.includes('@')) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Ingrese un correo electrónico válido.',
          path: ['applicant', 'email'],
        });
      }
    }
  });

export type PqrsdfFormValues = z.infer<typeof pqrsdfSchema>;

export function usePqrsdfForm(onSuccess?: (response: MakePqrsdfResponse) => void) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  const form = useForm<PqrsdfFormValues>({
    resolver: zodResolver(pqrsdfSchema) as Resolver<PqrsdfFormValues>,
    defaultValues: {
      type: 'Petition',
      destinationAreaId: '',
      isAnonymous: false,
      applicant: {
        fullName: '',
        identificationType: 'CC',
        identificationNumber: '',
        email: '',
        phoneNumber: '',
      },
      subject: '',
      description: '',
    },
    mode: 'onTouched',
  });

  const selectedType = form.watch('type') as PqrsdfType;
  const isAnonymous = form.watch('isAnonymous');
  const allowsAnonymous = selectedType === 'Denunciation' || selectedType === 'Suggestion';

  const onSubmit = async (values: PqrsdfFormValues) => {
    setIsSubmitting(true);
    setServerError(null);

    const payload: MakePqrsdfRequest = {
      type: values.type as PqrsdfType,
      destinationAreaId: values.destinationAreaId,
      isAnonymous: allowsAnonymous && values.isAnonymous,
      applicant:
        allowsAnonymous && values.isAnonymous
          ? null
          : {
              fullName: values.applicant!.fullName!.trim(),
              identificationType: values.applicant!.identificationType as IdentificationType,
              identificationNumber: values.applicant!.identificationNumber!.trim(),
              email: values.applicant!.email!.trim(),
              phoneNumber: values.applicant?.phoneNumber?.trim() || undefined,
            },
      subject: values.subject.trim(),
      description: values.description.trim(),
    };

    const result = await safeRequest<{ value: MakePqrsdfResponse }>(() =>
      rawClient.POST('/api/v1/pqrsdf', {
        body: payload,
      })
    );

    setIsSubmitting(false);

    if (result.ok) {
      const data = result.value;
      const response = ('value' in data && data.value ? data.value : data) as MakePqrsdfResponse;
      onSuccess?.({
        ...response,
        isAnonymous: payload.isAnonymous,
      });
    } else {
      setServerError(
        result.error.message || 'No fue posible registrar su solicitud. Intente nuevamente.'
      );
    }
  };

  return {
    form,
    isSubmitting,
    serverError,
    selectedType,
    isAnonymous,
    allowsAnonymous,
    handleSubmit: form.handleSubmit(onSubmit),
  };
}
