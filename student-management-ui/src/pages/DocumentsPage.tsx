import { useState } from 'react';
import { FileUploadDropzone } from '@/components/molecules/FileUploadDropzone';
import { OcrProgressBar } from '@/components/molecules/OcrProgressBar';
import { useOcrStore } from '@/store/ocrStore';
import { documentApi } from '@/api/documentApi';
import { Button } from '@/components/atoms/Button';

export function DocumentsPage() {
  const { resultJson, isActive } = useOcrStore();
  const [fileId, setFileId] = useState<string | null>(null);

  const handleFileSelected = (_file: File) => {
    setFileId(null);
  };

  const handleDownload = async () => {
    if (!fileId) return;
    const blob = await documentApi.download(fileId);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileId;
    a.click();
    URL.revokeObjectURL(url);
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

      {resultJson && (
        <div className="rounded-xl border border-gray-200 bg-gray-50 p-4">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-sm font-semibold text-gray-700">OCR Sonucu</h2>
            {fileId && (
              <Button size="sm" variant="secondary" onClick={handleDownload}>
                İndir
              </Button>
            )}
          </div>
          <pre className="overflow-x-auto text-xs text-gray-600 whitespace-pre-wrap">
            {typeof resultJson === 'string' ? resultJson : JSON.stringify(resultJson, null, 2)}
          </pre>
        </div>
      )}
    </div>
  );
}
