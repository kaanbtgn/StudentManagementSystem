import { useOcrStore } from '@/store/ocrStore';

export function OcrProgressBar() {
  const { isActive, progress, error } = useOcrStore();

  if (!isActive && !error) return null;

  return (
    <div className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
      <div className="mb-2 flex items-center justify-between text-sm">
        <span className="font-medium text-gray-700">
          {error ? 'OCR Başarısız' : (progress?.step ?? 'Başlatılıyor...')}
        </span>
        {!error && (
          <span className="text-gray-500">{progress?.progressPercent ?? 0}%</span>
        )}
      </div>
      {error ? (
        <p className="text-sm text-red-600">{error}</p>
      ) : (
        <div className="h-2 w-full overflow-hidden rounded-full bg-gray-200">
          <div
            className="h-full rounded-full bg-indigo-500 transition-all duration-300"
            style={{ width: `${progress?.progressPercent ?? 0}%` }}
          />
        </div>
      )}
    </div>
  );
}
