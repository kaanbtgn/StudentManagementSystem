export interface InternshipPaymentDto {
  id: string;
  studentId: string;
  studentName: string;
  periodYear: number;
  periodMonth: number;
  amount: number;
  paymentDate?: string;
  status: string;
}

export interface UpsertPaymentRequest {
  amount: number;
  paymentDate?: string;
}

export interface AmbiguousMatchItem {
  originalName: string;
  possibleMatches: string[];
}

export interface UpsertPaymentResult {
  processedCount: number;
  skippedCount: number;
  needsHumanVerification: boolean;
  ambiguousItems: AmbiguousMatchItem[];
}
