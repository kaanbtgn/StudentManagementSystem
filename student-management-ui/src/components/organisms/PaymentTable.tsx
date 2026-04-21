import type { InternshipPaymentDto } from '@/types/payment.types';
import { Badge, paymentStatusVariant } from '@/components/atoms/Badge';

interface PaymentTableProps {
  payments: InternshipPaymentDto[];
  onEdit: (payment: InternshipPaymentDto) => void;
}

const MONTHS = [
  '', 'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
  'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık',
];

export function PaymentTable({ payments, onEdit }: PaymentTableProps) {
  return (
    <div className="overflow-x-auto rounded-lg border border-gray-200">
      <table className="min-w-full divide-y divide-gray-200 text-sm">
        <thead className="bg-gray-50">
          <tr>
            {['Dönem', 'Tutar', 'Ödeme Tarihi', 'Durum', ''].map((h) => (
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
          {payments.map((p) => (
            <tr key={p.id} className="hover:bg-gray-50 transition-colors">
              <td className="px-4 py-3 text-gray-700">
                {MONTHS[p.periodMonth]} {p.periodYear}
              </td>
              <td className="px-4 py-3 font-medium text-gray-900">
                {p.amount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' })}
              </td>
              <td className="px-4 py-3 text-gray-600">{p.paymentDate ?? '—'}</td>
              <td className="px-4 py-3">
                <Badge label={p.status} variant={paymentStatusVariant(p.status)} />
              </td>
              <td className="px-4 py-3">
                <button
                  className="text-xs text-indigo-600 hover:underline"
                  onClick={() => onEdit(p)}
                >
                  Düzenle
                </button>
              </td>
            </tr>
          ))}
          {payments.length === 0 && (
            <tr>
              <td colSpan={5} className="px-4 py-8 text-center text-gray-400">
                Ödeme kaydı bulunamadı.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
