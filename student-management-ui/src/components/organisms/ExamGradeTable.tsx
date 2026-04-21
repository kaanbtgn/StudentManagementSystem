import type { ExamGradeDto } from '@/types/exam.types';

interface ExamGradeTableProps {
  grades: ExamGradeDto[];
  onEdit: (grade: ExamGradeDto) => void;
}

export function ExamGradeTable({ grades, onEdit }: ExamGradeTableProps) {
  return (
    <div className="overflow-x-auto rounded-lg border border-gray-200">
      <table className="min-w-full divide-y divide-gray-200 text-sm">
        <thead className="bg-gray-50">
          <tr>
            {['Ders Adı', '1. Sınav', '2. Sınav', ''].map((h) => (
              <th
                key={h}
                className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500"
              >
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100 bg-white">
          {grades.map((g) => (
            <tr key={g.id} className="hover:bg-gray-50 transition-colors">
              <td className="px-4 py-3 font-medium text-gray-900">{g.courseName}</td>
              <td className="px-4 py-3 text-gray-700">{g.exam1Grade ?? '—'}</td>
              <td className="px-4 py-3 text-gray-700">{g.exam2Grade ?? '—'}</td>
              <td className="px-4 py-3">
                <button
                  className="text-xs text-indigo-600 hover:underline"
                  onClick={() => onEdit(g)}
                >
                  Düzenle
                </button>
              </td>
            </tr>
          ))}
          {grades.length === 0 && (
            <tr>
              <td colSpan={4} className="px-4 py-8 text-center text-gray-400">
                Sınav notu bulunamadı.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
