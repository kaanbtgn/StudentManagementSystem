import { useCallback, useRef, useState } from 'react';
import { useOcrStore } from '@/store/ocrStore';
import { cn } from '@/lib/utils';

const MAX_SIZE_BYTES = 10 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif', 'application/pdf'];

interface FileUploadDropzoneProps {
  onFileSelected?: (file: File) => void;
}

export function FileUploadDropzone({ onFileSelected }: FileUploadDropzoneProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [validationError, setValidationError] = useState<string | null>(null);
  const { reset: resetOcr } = useOcrStore();

  const handleFile = useCallback(
    (file: File) => {
      setValidationError(null);
      if (!ALLOWED_TYPES.includes(file.type)) {
        setValidationError('Desteklenmeyen dosya türü. Lütfen resim veya PDF yükleyin.');
        return;
      }
      if (file.size > MAX_SIZE_BYTES) {
        setValidationError('Dosya boyutu 10 MB sınırını aşıyor.');
        return;
      }
      resetOcr();
      // Dosya state'e alınır; gönderim mesaj ile birlikte handleSend içinde yapılır
      onFileSelected?.(file);
    },
    [resetOcr, onFileSelected]
  );

  const onDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragging(false);
      const file = e.dataTransfer.files[0];
      if (file) handleFile(file);
    },
    [handleFile]
  );

  return (
    <div>
      <div
        className={cn(
          'flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-6 text-center transition-colors',
          isDragging
            ? 'border-indigo-400 bg-indigo-50'
            : 'border-gray-300 bg-gray-50 hover:border-indigo-400 hover:bg-indigo-50'
        )}
        onDragOver={(e) => { e.preventDefault(); setIsDragging(true); }}
        onDragLeave={() => setIsDragging(false)}
        onDrop={onDrop}
        onClick={() => inputRef.current?.click()}
        role="button"
        tabIndex={0}
        onKeyDown={(e) => e.key === 'Enter' && inputRef.current?.click()}
      >
        <svg className="mb-2 h-8 w-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
        </svg>
        <p className="text-sm text-gray-600">
          Dosyayı sürükleyip bırakın veya <span className="font-medium text-indigo-600">seçin</span>
        </p>
        <p className="mt-1 text-xs text-gray-400">PNG, JPG, WebP, PDF — maks. 10 MB</p>
        <input
          ref={inputRef}
          type="file"
          className="hidden"
          accept="image/*,application/pdf"
          onChange={(e) => { const f = e.target.files?.[0]; if (f) handleFile(f); }}
        />
      </div>
      {validationError && (
        <p className="mt-1 text-xs text-red-600">{validationError}</p>
      )}
    </div>
  );
}
