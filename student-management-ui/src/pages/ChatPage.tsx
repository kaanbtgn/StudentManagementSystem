import { useState } from 'react';
import { ChatPanel } from '@/components/organisms/ChatPanel';
import { OcrProgressBar } from '@/components/molecules/OcrProgressBar';
import { FileUploadDropzone } from '@/components/molecules/FileUploadDropzone';
import { Spinner } from '@/components/atoms/Spinner';
import { useChatStore } from '@/store/chatStore';
import { useSignalR } from '@/hooks/useSignalR';
import { agentApi } from '@/api/agentApi';

export function ChatPage() {
  const { sessionId, addUserMessage, isThinking } = useChatStore();
  const [input, setInput] = useState('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [sending, setSending] = useState(false);

  useSignalR(sessionId);

  const handleSend = async () => {
    const message = input.trim();
    if (!message || sending) return;
    setInput('');
    const file = selectedFile;
    setSelectedFile(null);
    addUserMessage(message);
    setSending(true);
    try {
      if (file) {
        await agentApi.chatWithDocument(message, file);
      } else {
        await agentApi.chat(message);
      }
    } finally {
      setSending(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="flex flex-1 flex-col overflow-hidden">
      <header className="border-b border-gray-200 bg-white px-6 py-4">
        <h1 className="text-base font-semibold text-gray-900">AI Asistan</h1>
      </header>

      <ChatPanel />

      <div className="border-t border-gray-200 bg-white px-4 py-3">
        <OcrProgressBar />
        {selectedFile && (
          <div className="mb-2 flex items-center gap-2 rounded-md bg-indigo-50 px-3 py-1.5 text-sm text-indigo-700">
            <span>📎 {selectedFile.name}</span>
            <button
              className="ml-auto text-indigo-400 hover:text-indigo-600"
              onClick={() => setSelectedFile(null)}
            >
              ✕
            </button>
          </div>
        )}
        <div className="flex items-end gap-2">
          <FileUploadDropzone onFileSelected={setSelectedFile} />
          <textarea
            rows={1}
            className="flex-1 resize-none rounded-xl border border-gray-300 px-4 py-2.5 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
            placeholder="Mesajınızı yazın... (Enter ile gönder)"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={handleKeyDown}
            disabled={isThinking || sending}
          />
          <button
            className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-600 text-white transition-colors hover:bg-indigo-700 disabled:opacity-50"
            onClick={handleSend}
            disabled={!input.trim() || isThinking || sending}
          >
            {sending ? <Spinner size="sm" className="text-white" /> : '↑'}
          </button>
        </div>
      </div>
    </div>
  );
}
