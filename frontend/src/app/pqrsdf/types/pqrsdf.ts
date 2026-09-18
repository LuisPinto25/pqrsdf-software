export type PqrsdfType =
  'Petition' | 'Complaint' | 'Claim' | 'Suggestion' | 'Denunciation' | 'Compliment';

export type IdentificationType = 'CC' | 'CE' | 'TI' | 'PA' | 'NIT';

export interface DestinationAreaDto {
  id: string;
  name: string;
  code: string;
}

export interface ApplicantInput {
  fullName: string;
  identificationType: IdentificationType;
  identificationNumber: string;
  email: string;
  phoneNumber?: string;
}

export interface MakePqrsdfRequest {
  type: PqrsdfType;
  destinationAreaId: string;
  isAnonymous: boolean;
  applicant?: ApplicantInput | null;
  subject: string;
  description: string;
}

export interface MakePqrsdfResponse {
  id: string;
  radicadoNumber: string;
  type: PqrsdfType;
  createdAtUtc: string;
  dueDate: string;
  businessDaysCount: number;
  status: string;
  destinationAreaName?: string;
  isAnonymous?: boolean;
}
