import { useMemo } from 'react';
import { FileUploadDropzone } from '@/components/molecules/FileUploadDropzone';
import { OcrProgressBar } from '@/components/molecules/OcrProgressBar';
import { useOcrStore } from '@/store/ocrStore';
import { documentApi } from '@/api/documentApi';
import { useSignalR } from '@/hooks/useSignalR';

export function DocumentsPage() {
  const { resultJson, isActive, error } = useOcrStore();
  const sessionId = useMemo(() => crypto.randomUUID(), []);

  useSignalR(sessionId);

  const handleFileSelected = async (file: File) => {
    await documentApi.uploadAsync(file, sessionId);
  };

  return (
    <div className="mx-auto max-w-2xl p-8">
      <h1 className="mb-6 text-xl font-semibold text-gray-900">Belge Yükle ve OCR</h1>

      <div className="mb-6 rounded-xl border-2 border-dashed border-gray-300 p-6">
        <FileUploadDropzone onFileSelected={handleFileSelected} />
      </div>

      {isActive && (
        <div className="mb-6">
          <OcrProgressBar />
        </div>
      )}

      {error && (
        <p className="mb-4 text-sm text-red-600">{error}</p>
      )}

      {resultJson && (
        <div className="rounded-xl border border-gray-200 bg-gray-50 p-4">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-sm font-semibold text-gray-700">OCR Sonucu</h2>
          </div>
          <pre className="overflow-x-auto text-xs text-gray-600 whitespace-pre-wrap">
            {typeof resultJson === 'string' ? resultJson : JSON.stringify(resultJson, null, 2)}
          </pre>
        </div>
      )}
    </div>
  );
}
