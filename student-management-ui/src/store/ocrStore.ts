import { create } from 'zustand';
import type { OcrProgress } from '@/types/agent.types';

interface OcrState {
  isActive: boolean;
  progress: OcrProgress | null;
  resultJson: string | null;
  error: string | null;
  setProgress: (progress: OcrProgress) => void;
  setCompleted: (resultJson: string) => void;
  setFailed: (error: string) => void;
  reset: () => void;
}

export const useOcrStore = create<OcrState>((set) => ({
  isActive: false,
  progress: null,
  resultJson: null,
  error: null,

  setProgress: (progress) => set({ isActive: true, progress, error: null }),

  setCompleted: (resultJson) =>
    set({ isActive: false, resultJson, progress: { step: 'Tamamlandı', progressPercent: 100 } }),

  setFailed: (error) => set({ isActive: false, error }),

  reset: () => set({ isActive: false, progress: null, resultJson: null, error: null }),
}));
