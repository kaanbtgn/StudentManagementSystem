import axiosInstance from './axiosInstance';
import type { CreateSessionResponse, SessionMessage, SessionSummary } from '@/types/session.types';

export const sessionApi = {
  create: () =>
    axiosInstance.post<CreateSessionResponse>('/api/sessions').then((r) => r.data),

  list: () =>
    axiosInstance.get<SessionSummary[]>('/api/sessions').then((r) => r.data),

  getMessages: (sessionId: string) =>
    axiosInstance
      .get<SessionMessage[]>(`/api/sessions/${sessionId}/messages`)
      .then((r) => r.data),

  delete: (sessionId: string) =>
    axiosInstance.delete(`/api/sessions/${sessionId}`).then((r) => r.data),
};
