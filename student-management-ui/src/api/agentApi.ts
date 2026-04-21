import axiosInstance from './axiosInstance';
import { getCurrentSessionId } from '@/store/sessionStore';

export const agentApi = {
  // sessionId is injected automatically via X-Session-Id header in axiosInstance
  chat: (message: string) =>
    axiosInstance.post<void>('/api/chat', { message }).then((r) => r.data),

  chatWithDocument: (message: string, file: File) => {
    const sessionId = getCurrentSessionId();
    const formData = new FormData();
    formData.append('sessionId', sessionId);
    formData.append('message', message);
    formData.append('file', file);
    return axiosInstance
      .post<void>('/api/chat/document', formData)
      .then((r) => r.data);
  },
};
