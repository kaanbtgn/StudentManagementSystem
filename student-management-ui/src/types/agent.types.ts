export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
  isStreaming?: boolean;
}

export interface OcrProgress {
  step: string;
  progressPercent: number;
}

export interface ChatRequest {
  message: string;
  sessionId: string;
}

export interface UploadAsyncResponse {
  jobId: string;
  sessionId: string;
}
