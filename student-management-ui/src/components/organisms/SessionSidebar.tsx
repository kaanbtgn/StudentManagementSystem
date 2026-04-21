import { useEffect, useState } from 'react';
import { useSessionStore } from '@/store/sessionStore';
import { useChatStore } from '@/store/chatStore';
import { ConfirmationDialog } from '@/components/molecules/ConfirmationDialog';
import { Spinner } from '@/components/atoms/Spinner';

interface SessionSidebarProps {
  onSessionChange: () => void;
}

export function SessionSidebar({ onSessionChange }: SessionSidebarProps) {
  const {
    sessions,
    currentSessionId,
    loading,
    fetchSessions,
    createSession,
    switchSession,
    deleteSession,
  } = useSessionStore();
  const { loadSession, clearHistory } = useChatStore();

  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [switching, setSwitching] = useState(false);

  useEffect(() => {
    const init = async () => {
      await fetchSessions();

      // If no session persisted in localStorage, create one automatically
      const { currentSessionId, sessions, createSession: create } = useSessionStore.getState();
      if (!currentSessionId && sessions.length === 0) {
        await create();
        onSessionChange();
      } else if (!currentSessionId && sessions.length > 0) {
        // Restore to most recent
        const messages = await switchSession(sessions[0].sessionId);
        loadSession(messages);
        onSessionChange();
      }
    };
    init();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleNew = async () => {
    clearHistory();
    const sessionId = await createSession();
    onSessionChange();
    return sessionId;
  };

  const handleSwitch = async (sessionId: string) => {
    if (sessionId === currentSessionId || switching) return;
    setSwitching(true);
    try {
      const messages = await switchSession(sessionId);
      loadSession(messages);
      onSessionChange();
    } finally {
      setSwitching(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!deletingId) return;
    const wasActive = deletingId === currentSessionId;
    await deleteSession(deletingId);
    setDeletingId(null);

    if (wasActive) {
      const next = useSessionStore.getState().sessions[0];
      if (next) {
        const messages = await switchSession(next.sessionId);
        loadSession(messages);
      } else {
        clearHistory();
        await createSession();
      }
      onSessionChange();
    }
  };

  return (
    <div className="flex w-56 shrink-0 flex-col border-r border-gray-200 bg-gray-50">
      <div className="border-b border-gray-200 p-3">
        <button
          onClick={handleNew}
          className="flex w-full items-center justify-center gap-2 rounded-lg bg-indigo-600 px-3 py-2 text-sm font-medium text-white transition-colors hover:bg-indigo-700 active:bg-indigo-800"
        >
          <span className="text-base leading-none">+</span>
          Yeni Sohbet
        </button>
      </div>

      <div className="flex flex-1 flex-col overflow-y-auto py-2">
        {loading && sessions.length === 0 ? (
          <div className="flex justify-center pt-6">
            <Spinner size="sm" />
          </div>
        ) : sessions.length === 0 ? (
          <p className="px-3 pt-4 text-center text-xs text-gray-400">
            Henüz sohbet yok.
          </p>
        ) : (
          sessions.map((session) => {
            const isActive = session.sessionId === currentSessionId;
            return (
              <div
                key={session.sessionId}
                className={`group relative flex cursor-pointer items-center gap-1 px-3 py-2 transition-colors ${
                  isActive
                    ? 'bg-indigo-50 text-indigo-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`}
                onClick={() => handleSwitch(session.sessionId)}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => e.key === 'Enter' && handleSwitch(session.sessionId)}
              >
                <div className="min-w-0 flex-1">
                  <p className="truncate text-xs font-medium leading-snug">
                    {session.title}
                  </p>
                  <p className="text-[10px] text-gray-400">
                    {session.messageCount} mesaj
                  </p>
                </div>

                <button
                  className="ml-1 shrink-0 rounded p-0.5 text-gray-300 opacity-0 transition-opacity hover:text-red-500 group-hover:opacity-100"
                  onClick={(e) => {
                    e.stopPropagation();
                    setDeletingId(session.sessionId);
                  }}
                  title="Sil"
                  aria-label="Sohbeti sil"
                >
                  🗑
                </button>
              </div>
            );
          })
        )}
      </div>

      {deletingId && (
        <ConfirmationDialog
          title="Sohbeti Sil"
          description="Bu sohbet kalıcı olarak silinecek. Devam etmek istiyor musunuz?"
          confirmLabel="Sil"
          cancelLabel="İptal"
          onConfirm={handleDeleteConfirm}
          onCancel={() => setDeletingId(null)}
        />
      )}
    </div>
  );
}
