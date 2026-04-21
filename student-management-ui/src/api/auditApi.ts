import axiosInstance from './axiosInstance';
import type { AuditEntry } from '@/types/audit.types';

export const auditApi = {
  getByStudent: (studentId: string) =>
    axiosInstance
      .get<AuditEntry[]>(`/api/audit/students/${studentId}`)
      .then((r) => r.data),
};
