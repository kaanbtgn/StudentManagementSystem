import { create } from 'zustand';
import type { ChatMessage } from '@/types/agent.types';

interface ChatState {
  messages: ChatMessage[];
  isThinking: boolean;
  isStreaming: boolean;
  currentStreamingContent: string;
  sessionId: string;
  setThinking: (value: boolean) => void;
  appendToken: (token: string) => void;
  setAssistantMessage: (content: string) => void;
  addUserMessage: (content: string) => void;
  setError: (error: string) => void;
  clearHistory: () => void;
}

function getOrCreateSessionId(): string {
  let id = localStorage.getItem('sessionId');
  if (!id) {
    id = crypto.randomUUID();
    localStorage.setItem('sessionId', id);
  }
  return id;
}

export const useChatStore = create<ChatState>((set) => ({
  messages: [],
  isThinking: false,
  isStreaming: false,
  currentStreamingContent: '',
  sessionId: getOrCreateSessionId(),

  setThinking: (value) => set({ isThinking: value }),

  appendToken: (token) =>
    set((state) => ({
      isStreaming: true,
      currentStreamingContent: state.currentStreamingContent + token,
    })),

  setAssistantMessage: (content) =>
    set((state) => ({
      isThinking: false,
      isStreaming: false,
      currentStreamingContent: '',
      messages: [
        ...state.messages,
        {
          id: crypto.randomUUID(),
          role: 'assistant',
          content,
          timestamp: new Date().toISOString(),
        },
      ],
    })),

  addUserMessage: (content) =>
    set((state) => ({
      messages: [
        ...state.messages,
        {
          id: crypto.randomUUID(),
          role: 'user',
          content,
          timestamp: new Date().toISOString(),
        },
      ],
    })),

  setError: (error) =>
    set((state) => ({
      isThinking: false,
      isStreaming: false,
      messages: [
        ...state.messages,
        {
          id: crypto.randomUUID(),
          role: 'assistant',
          content: `Hata: ${error}`,
          timestamp: new Date().toISOString(),
        },
      ],
    })),

  clearHistory: () =>
    set({ messages: [], isThinking: false, isStreaming: false, currentStreamingContent: '' }),
}));
