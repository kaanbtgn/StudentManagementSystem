import axiosInstance from './axiosInstance';
import type { UploadAsyncResponse } from '@/types/agent.types';

export const documentApi = {
  uploadAsync: (file: File, sessionId: string) => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('sessionId', sessionId);
    return axiosInstance
      .post<UploadAsyncResponse>('/api/documents/upload-async', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data);
  },

  download: (fileId: string) =>
    axiosInstance
      .get(`/api/docs/${fileId}`, { responseType: 'blob' })
      .then((r) => r.data as Blob),
};
