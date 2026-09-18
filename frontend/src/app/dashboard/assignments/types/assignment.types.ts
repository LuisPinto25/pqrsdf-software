export type UrgencyLevel = 'Critical' | 'Attention' | 'OnTime';

export interface UnassignedTicketDto {
  id: string;
  radicadoNumber: string;
  type: number;
  typeName: string;
  destinationAreaId: string;
  destinationAreaName: string;
  subject: string;
  description: string;
  filingDateUtc: string;
  dueDateUtc: string;
  remainingBusinessDays: number;
  urgencyLevel: UrgencyLevel;
}

export interface AssignedTicketDto {
  id: string;
  radicadoNumber: string;
  type: number;
  typeName: string;
  destinationAreaName: string;
  subject: string;
  description?: string | null;
  assignedToUserId: string;
  assignedOfficialName: string;
  assignedAtUtc: string;
  assignmentNote?: string | null;
  dueDateUtc: string;
  remainingBusinessDays: number;
  urgencyLevel: UrgencyLevel;
}

export interface AssignableOfficialDto {
  id: string;
  fullName: string;
  email: string;
  activeTicketsCount: number;
  maxCapacity: number;
  canAssign: boolean;
}

export interface AssignTicketRequest {
  officialId: string;
  note?: string | null;
}

export interface AssignTicketResponse {
  radicadoNumber: string;
  status: string;
  assignedToUserId: string;
  assignedOfficialName: string;
  assignedAtUtc: string;
  message: string;
}

export interface ReassignTicketRequest {
  newOfficialId: string;
  justification: string;
}

export interface ReassignTicketResponse {
  radicadoNumber: string;
  previousAssignedUserId?: string | null;
  newAssignedUserId: string;
  newOfficialName: string;
  reassignedAtUtc: string;
  message: string;
}

export interface OfficialInboxItemDto {
  id: string;
  radicadoNumber: string;
  type: number;
  typeName: string;
  destinationAreaName: string;
  subject: string;
  description: string;
  assignedAtUtc: string;
  assignmentNote?: string | null;
  dueDateUtc: string;
  remainingBusinessDays: number;
  urgencyLevel: UrgencyLevel;
}

export interface OfficialInboxResponse {
  activeCount: number;
  maxCapacity: number;
  tickets: OfficialInboxItemDto[];
}
