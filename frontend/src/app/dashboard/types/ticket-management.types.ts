import { UrgencyLevel } from '../assignments/types/assignment.types';

export interface ApplicantDetailDto {
  documentType: string;
  documentNumber: string;
  fullName: string;
  email: string;
  phone?: string | null;
}

export interface TicketStatusHistoryItemDto {
  id: string;
  previousStatus: number;
  previousStatusName: string;
  newStatus: number;
  newStatusName: string;
  changedByOfficialName: string;
  justification: string;
  changedAtUtc: string;
}

export interface TicketManagementDetailDto {
  id: string;
  radicadoNumber: string;
  type: number;
  typeName: string;
  destinationAreaId: string;
  destinationAreaName: string;
  isAnonymous: boolean;
  applicant: ApplicantDetailDto | null;
  subject: string;
  description: string;
  filingDateUtc: string;
  dueDateUtc: string;
  remainingBusinessDays: number | null;
  urgencyLevel: UrgencyLevel;
  status: number;
  statusName: string;
  assignedToUserId: string | null;
  assignedOfficialName: string | null;
  assignedAtUtc: string | null;
  assignmentNote: string | null;
  responseText: string | null;
  responseDateUtc: string | null;
  isAssignedToCurrentUser: boolean;
  canManage: boolean;
  statusHistory: TicketStatusHistoryItemDto[];
}

export interface RespondTicketRequest {
  responseText: string;
}

export interface RespondTicketResponse {
  radicadoNumber: string;
  status: number;
  statusName: string;
  responseDateUtc: string;
  closedByOfficialName: string;
  message: string;
}

export interface ChangeTicketStatusRequest {
  newStatus: number;
  justification: string;
}

export interface ChangeTicketStatusResponse {
  radicadoNumber: string;
  previousStatus: number;
  newStatus: number;
  statusName: string;
  updatedAtUtc: string;
  changedByOfficialName: string;
  justification: string;
}
