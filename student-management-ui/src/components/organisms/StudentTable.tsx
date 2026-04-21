import { useState } from 'react';
import type { StudentDto } from '@/types/student.types';
import { Button } from '@/components/atoms/Button';
import { ConfirmationDialog } from '@/components/molecules/ConfirmationDialog';
import { useStudentStore } from '@/store/studentStore';

interface StudentTableProps {
  onEdit: (student: StudentDto) => void;
  onDelete: (id: string) => void;
  onSelect: (student: StudentDto) => void;
}

export function StudentTable({ onEdit, onDelete, onSelect }: StudentTableProps) {
  const { students } = useStudentStore();
  const [deletingId, setDeletingId] = useState<string | null>(null);

  return (
    <>
      <div className="overflow-x-auto rounded-lg border border-gray-200">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              {['Öğrenci No', 'Ad Soyad', 'Bölüm', 'Telefon', 'Kayıt Tarihi', 'İşlemler'].map(
                (h) => (
                  <th
                    key={h}
                    className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
                  >
                    {h}
                  </th>
                )
              )}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 bg-white">
            {students.map((student) => (
              <tr
                key={student.id}
                className="cursor-pointer hover:bg-indigo-50 transition-colors"
                onClick={() => onSelect(student)}
              >
                <td className="px-4 py-3 font-mono text-xs text-gray-700">
                  {student.studentNumber}
                </td>
                <td className="px-4 py-3 font-medium text-gray-900">
                  {student.firstName} {student.lastName}
                </td>
                <td className="px-4 py-3 text-gray-600">{student.department}</td>
                <td className="px-4 py-3 text-gray-600">{student.phone ?? '—'}</td>
                <td className="px-4 py-3 text-gray-600">{student.enrollmentDate}</td>
                <td className="px-4 py-3">
                  <div className="flex gap-2" onClick={(e) => e.stopPropagation()}>
                    <Button size="sm" variant="secondary" onClick={() => onEdit(student)}>
                      Düzenle
                    </Button>
                    <Button
                      size="sm"
                      variant="danger"
                      onClick={() => setDeletingId(student.id)}
                    >
                      Sil
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
            {students.length === 0 && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-gray-400">
                  Öğrenci bulunamadı.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {deletingId && (
        <ConfirmationDialog
          title="Öğrenciyi Sil"
          description="Bu öğrenciyi silmek istediğinizden emin misiniz? Bu işlem geri alınamaz."
          confirmLabel="Evet, Sil"
          onConfirm={() => { onDelete(deletingId); setDeletingId(null); }}
          onCancel={() => setDeletingId(null)}
        />
      )}
    </>
  );
}
