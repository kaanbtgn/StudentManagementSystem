export interface ExamGradeDto {
  id: string;
  studentId: string;
  studentName: string;
  courseName: string;
  exam1Grade?: number;
  exam2Grade?: number;
}

export interface UpsertExamGradeRequest {
  exam1Grade?: number;
  exam2Grade?: number;
}

export interface AmbiguousGradeItem {
  originalName: string;
  possibleMatches: string[];
}

export interface UpsertGradeResult {
  processedCount: number;
  needsHumanVerification: boolean;
  ambiguousItems: AmbiguousGradeItem[];
}
