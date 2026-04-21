import { useEffect, useRef } from 'react';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';
import { useChatStore } from '@/store/chatStore';
import { useOcrStore } from '@/store/ocrStore';
import type { OcrProgress } from '@/types/agent.types';

export function useSignalR(sessionId: string) {
  const connection = useRef<HubConnection | null>(null);
  const { setThinking, setAssistantMessage, setError, appendToken } = useChatStore();
  const { setProgress, setCompleted, setFailed } = useOcrStore();

  useEffect(() => {
    if (!sessionId) return;

    const hub = new HubConnectionBuilder()
      .withUrl(import.meta.env.VITE_SIGNALR_HUB_URL)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.current = hub;

    hub.on('AgentThinking', () => setThinking(true));
    hub.on('MessageReceived', () => setThinking(false));
    hub.on('AgentResponseCompleted', (payload: { response: string }) =>
      setAssistantMessage(payload.response)
    );
    hub.on('AgentError', (payload: { error: string }) => setError(payload.error));
    hub.on('AgentTokenReceived', (payload: { token: string }) => appendToken(payload.token));
    hub.on('OcrProgressUpdated', (payload: OcrProgress) => setProgress(payload));
    hub.on('OcrCompleted', (payload: { result: string }) => setCompleted(payload.result));
    hub.on('OcrFailed', (payload: { error: string }) => setFailed(payload.error));

    hub
      .start()
      .then(() => hub.invoke('JoinSession', sessionId))
      .catch((err) => console.error('[SignalR] Connection error:', err));

    return () => {
      hub
        .invoke('LeaveSession', sessionId)
        .finally(() => hub.stop())
        .catch(() => hub.stop());
    };
  }, [sessionId, setThinking, setAssistantMessage, setError, appendToken, setProgress, setCompleted, setFailed]);
}
