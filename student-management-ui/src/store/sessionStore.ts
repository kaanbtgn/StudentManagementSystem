import { create } from 'zustand';
import { sessionApi } from '@/api/sessionApi';
import type { SessionSummary, SessionMessage } from '@/types/session.types';

const SESSION_KEY = 'sessionId';

function persistedSessionId(): string {
  return localStorage.getItem(SESSION_KEY) ?? '';
}

interface SessionState {
  sessions: SessionSummary[];
  currentSessionId: string;
  loading: boolean;

  fetchSessions: () => Promise<void>;
  createSession: () => Promise<string>;
  switchSession: (sessionId: string) => SessionMessage[] | Promise<SessionMessage[]>;
  deleteSession: (sessionId: string) => Promise<void>;
  updateCurrentSessionId: (sessionId: string) => void;
}

export const useSessionStore = create<SessionState>((set) => ({
  sessions: [],
  currentSessionId: persistedSessionId(),
  loading: false,

  fetchSessions: async () => {
    set({ loading: true });
    try {
      const sessions = await sessionApi.list();
      set({ sessions, loading: false });
    } catch {
      set({ loading: false });
    }
  },

  createSession: async () => {
    const { sessionId } = await sessionApi.create();
    localStorage.setItem(SESSION_KEY, sessionId);
    set((state) => ({
      currentSessionId: sessionId,
      sessions: [
        {
          sessionId,
          title: 'Yeni Sohbet',
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          messageCount: 0,
        },
        ...state.sessions,
      ],
    }));
    return sessionId;
  },

  switchSession: async (sessionId: string) => {
    localStorage.setItem(SESSION_KEY, sessionId);
    set({ currentSessionId: sessionId });
    return sessionApi.getMessages(sessionId);
  },

  deleteSession: async (sessionId: string) => {
    await sessionApi.delete(sessionId);
    set((state) => {
      const remaining = state.sessions.filter((s) => s.sessionId !== sessionId);
      let currentSessionId = state.currentSessionId;

      if (currentSessionId === sessionId) {
        currentSessionId = remaining[0]?.sessionId ?? '';
        localStorage.setItem(SESSION_KEY, currentSessionId);
      }

      return { sessions: remaining, currentSessionId };
    });
  },

  updateCurrentSessionId: (sessionId: string) => {
    localStorage.setItem(SESSION_KEY, sessionId);
    set({ currentSessionId: sessionId });
  },
}));

export function getCurrentSessionId(): string {
  return useSessionStore.getState().currentSessionId || persistedSessionId();
}
