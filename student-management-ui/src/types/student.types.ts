export interface StudentDto {
  id: string;
  firstName: string;
  lastName: string;
  studentNumber: string;
  department: string;
  phone?: string;
  enrollmentDate: string;
}

export interface CreateStudentRequest {
  firstName: string;
  lastName: string;
  studentNumber: string;
  department: string;
  phone?: string;
  enrollmentDate: string;
}

export interface UpdateStudentRequest {
  firstName?: string;
  lastName?: string;
  department?: string;
  phone?: string;
}

export interface FuzzyMatchItem {
  value: string;
  score: number;
}

export interface FuzzyMatchResponse {
  matches: FuzzyMatchItem[];
  requiresConfirmation: boolean;
}
