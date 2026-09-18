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

export interface TicketTimelineMilestoneDto {
  status: string;
  title: string;
  date: string | null;
  isCompleted: boolean;
  isCurrent: boolean;
}

export interface TicketResolutionDto {
  responseText: string;
  responseDate: string;
}

export interface PublicTicketStatusDto {
  radicadoNumber: string;
  requestType: string;
  destinationAreaName: string;
  subject: string;
  description: string;
  status: string;
  filingDate: string;
  dueDate: string;
  remainingBusinessDays: number | null;
  isOverdue: boolean;
  overdueBusinessDays: number | null;
  timeline: TicketTimelineMilestoneDto[];
  resolution?: TicketResolutionDto | null;
}
