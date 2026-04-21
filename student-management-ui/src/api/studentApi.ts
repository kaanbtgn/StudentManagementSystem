import axiosInstance from './axiosInstance';
import type {
  StudentDto,
  CreateStudentRequest,
  UpdateStudentRequest,
  FuzzyMatchResponse,
} from '@/types/student.types';

export const studentApi = {
  getAll: () =>
    axiosInstance.get<StudentDto[]>('/api/students').then((r) => r.data),

  getById: (id: string) =>
    axiosInstance.get<StudentDto>(`/api/students/${id}`).then((r) => r.data),

  search: (term: string) =>
    axiosInstance
      .get<StudentDto[]>('/api/students/search', { params: { term } })
      .then((r) => r.data),

  create: (payload: CreateStudentRequest) =>
    axiosInstance.post<StudentDto>('/api/students', payload).then((r) => r.data),

  update: (id: string, payload: UpdateStudentRequest) =>
    axiosInstance.put<StudentDto>(`/api/students/${id}`, payload).then((r) => r.data),

  remove: (id: string) =>
    axiosInstance.delete(`/api/students/${id}`).then((r) => r.data),

  fuzzyMatch: (name: string) =>
    axiosInstance
      .post<FuzzyMatchResponse>('/api/students/fuzzy-match', { name })
      .then((r) => r.data),
};
