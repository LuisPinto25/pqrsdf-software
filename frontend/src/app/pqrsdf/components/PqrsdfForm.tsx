'use client';

import React from 'react';
import { useTranslations } from 'next-intl';
import { Send, AlertCircle, Loader2 } from 'lucide-react';
import { useDestinationAreas } from '../hooks/useDestinationAreas';
import { usePqrsdfForm } from '../hooks/usePqrsdfForm';
import { CharacterCounter } from './CharacterCounter';
import type { MakePqrsdfResponse, PqrsdfType, IdentificationType } from '../types/pqrsdf';

interface PqrsdfFormProps {
  onSuccess: (response: MakePqrsdfResponse) => void;
}

const PQRSDF_TYPES: { type: PqrsdfType; labelKey: string }[] = [
  { type: 'Petition', labelKey: 'Petition' },
  { type: 'Complaint', labelKey: 'Complaint' },
  { type: 'Claim', labelKey: 'Claim' },
  { type: 'Suggestion', labelKey: 'Suggestion' },
  { type: 'Denunciation', labelKey: 'Denunciation' },
  { type: 'Compliment', labelKey: 'Compliment' },
];

const ID_TYPES: IdentificationType[] = ['CC', 'CE', 'TI', 'PA', 'NIT'];

export const PqrsdfForm: React.FC<PqrsdfFormProps> = ({ onSuccess }) => {
  const t = useTranslations('Pqrsdf');
  const { areas, isLoading: loadingAreas, error: areasError } = useDestinationAreas();

  const {
    form,
    isSubmitting,
    serverError,
    selectedType,
    isAnonymous,
    allowsAnonymous,
    handleSubmit,
  } = usePqrsdfForm(onSuccess);

  const {
    register,
    watch,
    formState: { errors },
  } = form;

  const subjectValue = watch('subject') || '';
  const descriptionValue = watch('description') || '';

  return (
    <form
      onSubmit={handleSubmit}
      className="bg-white border border-gray-200 rounded-xl shadow-sm p-6 sm:p-8 space-y-8"
      noValidate
    >
      {serverError && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg flex items-center space-x-3 text-sm">
          <AlertCircle className="w-5 h-5 flex-shrink-0 text-red-600" />
          <span>{serverError}</span>
        </div>
      )}

      {/* Seccion 1: Clasificación de la Solicitud */}
      <div className="space-y-6">
        <h2 className="text-lg font-bold text-gray-900 border-b pb-2">
          1. Clasificación del Trámite
        </h2>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {/* Tipo de solicitud */}
          <div>
            <label htmlFor="type" className="block text-sm font-semibold text-gray-800 mb-1">
              {t('form.requestType')} <span className="text-red-500">*</span>
            </label>
            <select
              id="type"
              {...register('type')}
              className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
            >
              {PQRSDF_TYPES.map(({ type, labelKey }) => (
                <option key={type} value={type}>
                  {t(`types.${labelKey}`)}
                </option>
              ))}
            </select>
            {errors.type && <p className="mt-1 text-xs text-red-600">{errors.type.message}</p>}
          </div>

          {/* Área destino */}
          <div>
            <label
              htmlFor="destinationAreaId"
              className="block text-sm font-semibold text-gray-800 mb-1"
            >
              {t('form.destinationArea')} <span className="text-red-500">*</span>
            </label>
            <select
              id="destinationAreaId"
              {...register('destinationAreaId')}
              disabled={loadingAreas}
              className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition disabled:opacity-60"
            >
              <option value="">
                {loadingAreas ? t('form.loadingAreas') : t('form.selectArea')}
              </option>
              {areas.map((area) => (
                <option key={area.id} value={area.id}>
                  {area.name} ({area.code})
                </option>
              ))}
            </select>
            {areasError && <p className="mt-1 text-xs text-red-600">{t('form.areasError')}</p>}
            {errors.destinationAreaId && (
              <p className="mt-1 text-xs text-red-600">{errors.destinationAreaId.message}</p>
            )}
          </div>
        </div>

        {/* Opción anónima (únicamente disponible para Denuncia o Sugerencia) */}
        {allowsAnonymous && (
          <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
            <label className="flex items-start space-x-3 cursor-pointer">
              <input
                type="checkbox"
                {...register('isAnonymous')}
                className="mt-1 h-4 w-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
              />
              <div>
                <span className="text-sm font-semibold text-blue-900">
                  {t('form.anonymousOption')} (
                  {selectedType === 'Denunciation' ? 'Denuncia' : 'Sugerencia'})
                </span>
                <p className="text-xs text-blue-700 mt-0.5">{t('form.anonymousHint')}</p>
              </div>
            </label>
          </div>
        )}
      </div>

      {/* Seccion 2: Datos del Solicitante (si no es anónimo) */}
      {!isAnonymous && (
        <div className="space-y-6">
          <h2 className="text-lg font-bold text-gray-900 border-b pb-2">
            2. {t('form.applicantSection')}
          </h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Nombre completo */}
            <div className="md:col-span-2">
              <label htmlFor="fullName" className="block text-sm font-semibold text-gray-800 mb-1">
                {t('form.fullName')} <span className="text-red-500">*</span>
              </label>
              <input
                id="fullName"
                type="text"
                {...register('applicant.fullName')}
                placeholder={t('form.fullNamePlaceholder')}
                className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
              />
              {errors.applicant?.fullName && (
                <p className="mt-1 text-xs text-red-600">{errors.applicant.fullName.message}</p>
              )}
            </div>

            {/* Tipo de Documento */}
            <div>
              <label htmlFor="idType" className="block text-sm font-semibold text-gray-800 mb-1">
                {t('form.identificationType')} <span className="text-red-500">*</span>
              </label>
              <select
                id="idType"
                {...register('applicant.identificationType')}
                className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
              >
                {ID_TYPES.map((idType) => (
                  <option key={idType} value={idType}>
                    {t(`idTypes.${idType}`)}
                  </option>
                ))}
              </select>
              {errors.applicant?.identificationType && (
                <p className="mt-1 text-xs text-red-600">
                  {errors.applicant.identificationType.message}
                </p>
              )}
            </div>

            {/* Número de Documento */}
            <div>
              <label htmlFor="idNumber" className="block text-sm font-semibold text-gray-800 mb-1">
                {t('form.identificationNumber')} <span className="text-red-500">*</span>
              </label>
              <input
                id="idNumber"
                type="text"
                {...register('applicant.identificationNumber')}
                placeholder={t('form.idNumberPlaceholder')}
                className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
              />
              {errors.applicant?.identificationNumber && (
                <p className="mt-1 text-xs text-red-600">
                  {errors.applicant.identificationNumber.message}
                </p>
              )}
            </div>

            {/* Correo Electrónico */}
            <div>
              <label htmlFor="email" className="block text-sm font-semibold text-gray-800 mb-1">
                {t('form.email')} <span className="text-red-500">*</span>
              </label>
              <input
                id="email"
                type="email"
                {...register('applicant.email')}
                placeholder={t('form.emailPlaceholder')}
                className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
              />
              <p className="text-xs text-gray-500 mt-1">{t('form.emailHint')}</p>
              {errors.applicant?.email && (
                <p className="mt-1 text-xs text-red-600">{errors.applicant.email.message}</p>
              )}
            </div>

            {/* Teléfono de Contacto */}
            <div>
              <label htmlFor="phone" className="block text-sm font-semibold text-gray-800 mb-1">
                {t('form.phone')}
              </label>
              <input
                id="phone"
                type="tel"
                {...register('applicant.phoneNumber')}
                placeholder={t('form.phonePlaceholder')}
                className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
              />
              {errors.applicant?.phoneNumber && (
                <p className="mt-1 text-xs text-red-600">{errors.applicant.phoneNumber.message}</p>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Seccion 3: Contenido de la Solicitud */}
      <div className="space-y-6">
        <h2 className="text-lg font-bold text-gray-900 border-b pb-2">
          {isAnonymous ? '2' : '3'}. {t('form.ticketSection')}
        </h2>

        {/* Asunto */}
        <div>
          <label htmlFor="subject" className="block text-sm font-semibold text-gray-800 mb-1">
            {t('form.subject')} <span className="text-red-500">*</span>
          </label>
          <input
            id="subject"
            type="text"
            maxLength={150}
            {...register('subject')}
            placeholder={t('form.subjectPlaceholder')}
            className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
          />
          <CharacterCounter currentLength={subjectValue.length} maxLength={150} minLength={5} />
          {errors.subject && <p className="mt-1 text-xs text-red-600">{errors.subject.message}</p>}
        </div>

        {/* Descripción */}
        <div>
          <label htmlFor="description" className="block text-sm font-semibold text-gray-800 mb-1">
            {t('form.description')} <span className="text-red-500">*</span>
          </label>
          <textarea
            id="description"
            rows={6}
            maxLength={4000}
            {...register('description')}
            placeholder={t('form.descriptionPlaceholder')}
            className="w-full px-3 py-2.5 bg-gray-50 border border-gray-300 rounded-lg text-gray-900 text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
          />
          <CharacterCounter
            currentLength={descriptionValue.length}
            maxLength={4000}
            minLength={10}
          />
          {errors.description && (
            <p className="mt-1 text-xs text-red-600">{errors.description.message}</p>
          )}
        </div>
      </div>

      {/* Botón de Envío */}
      <div className="pt-4 border-t border-gray-200">
        <button
          type="submit"
          disabled={isSubmitting}
          className="w-full sm:w-auto px-8 py-3 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition duration-150 flex items-center justify-center space-x-2 disabled:opacity-60 disabled:cursor-not-allowed cursor-pointer"
        >
          {isSubmitting ? (
            <>
              <Loader2 className="w-5 h-5 animate-spin" />
              <span>{t('form.submitting')}</span>
            </>
          ) : (
            <>
              <Send className="w-5 h-5" />
              <span>{t('form.submit')}</span>
            </>
          )}
        </button>
      </div>
    </form>
  );
};
