import { useRef, useEffect } from 'react';
import { ChatMessage, ThinkingIndicator } from '@/components/molecules/ChatMessage';
import { useChatStore } from '@/store/chatStore';

export function ChatPanel() {
  const { messages, isThinking } = useChatStore();
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isThinking]);

  return (
    <div className="flex flex-1 flex-col gap-4 overflow-y-auto px-4 py-4">
      {messages.length === 0 && (
        <div className="flex flex-1 items-center justify-center text-sm text-gray-400">
          Bir mesaj yazarak sohbete başlayın.
        </div>
      )}
      {messages.map((msg) => (
        <ChatMessage key={msg.id} message={msg} />
      ))}
      {isThinking && <ThinkingIndicator />}
      <div ref={bottomRef} />
    </div>
  );
}
