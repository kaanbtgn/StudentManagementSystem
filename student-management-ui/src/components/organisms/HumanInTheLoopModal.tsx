import { useState } from 'react';
import type { AmbiguousMatchItem } from '@/types/payment.types';
import type { AmbiguousGradeItem } from '@/types/exam.types';
import { Button } from '@/components/atoms/Button';

type AmbiguousItem = AmbiguousMatchItem | AmbiguousGradeItem;

interface HumanInTheLoopModalProps {
  items: AmbiguousItem[];
  onConfirm: (selections: Record<string, string>) => void;
  onCancel: () => void;
}

export function HumanInTheLoopModal({ items, onConfirm, onCancel }: HumanInTheLoopModalProps) {
  const [selections, setSelections] = useState<Record<string, string>>(() =>
    Object.fromEntries(items.map((item) => [item.originalName, item.possibleMatches[0] ?? '']))
  );

  const handleConfirm = () => onConfirm(selections);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-lg rounded-xl bg-white p-6 shadow-xl">
        <h3 className="mb-1 text-base font-semibold text-gray-900">Onay Gerekiyor</h3>
        <p className="mb-4 text-sm text-gray-500">
          Aşağıdaki eşleşmeler belirsiz. Lütfen doğru kaydı seçin.
        </p>
        <div className="flex flex-col gap-4">
          {items.map((item) => (
            <div key={item.originalName}>
              <label className="mb-1 block text-sm font-medium text-gray-700">
                {item.originalName}
              </label>
              <select
                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                value={selections[item.originalName]}
                onChange={(e) =>
                  setSelections((prev) => ({ ...prev, [item.originalName]: e.target.value }))
                }
              >
                {item.possibleMatches.map((match) => (
                  <option key={match} value={match}>
                    {match}
                  </option>
                ))}
              </select>
            </div>
          ))}
        </div>
        <div className="mt-6 flex justify-end gap-3">
          <Button variant="secondary" onClick={onCancel}>
            İptal
          </Button>
          <Button variant="primary" onClick={handleConfirm}>
            Onayla ve Kaydet
          </Button>
        </div>
      </div>
    </div>
  );
}
