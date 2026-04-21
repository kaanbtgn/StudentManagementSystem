import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { StudentTable } from '@/components/organisms/StudentTable';
import { AuditLogPanel } from '@/components/organisms/AuditLogPanel';
import { Input } from '@/components/atoms/Input';
import { Button } from '@/components/atoms/Button';
import { Spinner } from '@/components/atoms/Spinner';
import { useStudents } from '@/hooks/useStudents';
import { useStudentStore } from '@/store/studentStore';
import type { StudentDto, UpdateStudentRequest } from '@/types/student.types';
import { usePayments } from '@/hooks/usePayments';
import { useExamGrades } from '@/hooks/useExamGrades';
import { PaymentTable } from '@/components/organisms/PaymentTable';
import { ExamGradeTable } from '@/components/organisms/ExamGradeTable';
import { HumanInTheLoopModal } from '@/components/organisms/HumanInTheLoopModal';
import type { InternshipPaymentDto, UpsertPaymentResult } from '@/types/payment.types';
import type { ExamGradeDto, UpsertGradeResult } from '@/types/exam.types';

const createSchema = z.object({
  firstName: z.string().min(1, 'Ad zorunludur'),
  lastName: z.string().min(1, 'Soyad zorunludur'),
  studentNumber: z.string().min(1, 'Öğrenci numarası zorunludur'),
  department: z.string().min(1, 'Bölüm zorunludur'),
  enrollmentDate: z.string().min(1, 'Kayıt tarihi zorunludur'),
  phone: z.string().optional(),
});
type CreateFormData = z.infer<typeof createSchema>;

const editSchema = z.object({
  firstName: z.string().min(1),
  lastName: z.string().min(1),
  department: z.string().min(1),
  phone: z.string().optional(),
});
type EditFormData = z.infer<typeof editSchema>;

type DetailTab = 'payments' | 'exams' | 'audit';

function useDebounce<T>(value: T, delay: number): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);
  return debounced;
}

export function StudentsPage() {
  const { loading, error, fetchAll, search, create, update, remove } = useStudents();
  const { selectedStudent, setSelectedStudent } = useStudentStore();
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearch = useDebounce(searchTerm, 300);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingStudent, setEditingStudent] = useState<StudentDto | null>(null);
  const [detailTab, setDetailTab] = useState<DetailTab>('payments');
  const [humanLoopResult, setHumanLoopResult] = useState<
    (UpsertPaymentResult | UpsertGradeResult) | null
  >(null);

  const createForm = useForm<CreateFormData>({ resolver: zodResolver(createSchema) });
  const editForm = useForm<EditFormData>({ resolver: zodResolver(editSchema) });

  useEffect(() => { fetchAll(); }, [fetchAll]);
  useEffect(() => {
    if (debouncedSearch) search(debouncedSearch);
    else fetchAll();
  }, [debouncedSearch, search, fetchAll]);

  const handleCreate = createForm.handleSubmit(async (data) => {
    await create(data);
    createForm.reset();
    setShowCreateForm(false);
  });

  const handleEdit = editForm.handleSubmit(async (data: UpdateStudentRequest) => {
    if (!editingStudent) return;
    await update(editingStudent.id, data);
    setEditingStudent(null);
  });

  const handleDelete = async (id: string) => {
    await remove(id);
    if (selectedStudent?.id === id) setSelectedStudent(null);
  };

  return (
    <div className="flex flex-1 overflow-hidden">
      {/* Left: student list */}
      <div className="flex flex-1 flex-col overflow-hidden border-r border-gray-200 bg-gray-50">
        <header className="flex items-center justify-between border-b border-gray-200 bg-white px-6 py-4">
          <h1 className="text-base font-semibold text-gray-900">Öğrenciler</h1>
          <Button size="sm" onClick={() => setShowCreateForm(true)}>+ Yeni Öğrenci</Button>
        </header>

        <div className="px-6 py-3">
          <Input
            placeholder="Öğrenci ara..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        {loading && (
          <div className="flex justify-center py-8"><Spinner /></div>
        )}
        {error && <p className="px-6 text-sm text-red-600">{error}</p>}

        <div className="flex-1 overflow-y-auto px-6 pb-6">
          <StudentTable
            onEdit={(s) => { setEditingStudent(s); editForm.reset(s); }}
            onDelete={handleDelete}
            onSelect={(s) => { setSelectedStudent(s); setDetailTab('payments'); }}
          />
        </div>
      </div>

      {/* Right: detail panel */}
      {selectedStudent && (
        <DetailPanel
          student={selectedStudent}
          tab={detailTab}
          onTabChange={setDetailTab}
          onHumanLoop={setHumanLoopResult}
        />
      )}

      {/* Create form modal */}
      {showCreateForm && (
        <Modal title="Yeni Öğrenci" onClose={() => setShowCreateForm(false)}>
          <form onSubmit={handleCreate} className="flex flex-col gap-3">
            <Input label="Ad" {...createForm.register('firstName')} error={createForm.formState.errors.firstName?.message} />
            <Input label="Soyad" {...createForm.register('lastName')} error={createForm.formState.errors.lastName?.message} />
            <Input label="Öğrenci No" {...createForm.register('studentNumber')} error={createForm.formState.errors.studentNumber?.message} />
            <Input label="Bölüm" {...createForm.register('department')} error={createForm.formState.errors.department?.message} />
            <Input label="Kayıt Tarihi" type="date" {...createForm.register('enrollmentDate')} error={createForm.formState.errors.enrollmentDate?.message} />
            <Input label="Telefon (opsiyonel)" {...createForm.register('phone')} />
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="secondary" type="button" onClick={() => setShowCreateForm(false)}>İptal</Button>
              <Button type="submit">Kaydet</Button>
            </div>
          </form>
        </Modal>
      )}

      {/* Edit form modal */}
      {editingStudent && (
        <Modal title="Öğrenci Düzenle" onClose={() => setEditingStudent(null)}>
          <form onSubmit={handleEdit} className="flex flex-col gap-3">
            <Input label="Ad" {...editForm.register('firstName')} />
            <Input label="Soyad" {...editForm.register('lastName')} />
            <Input label="Bölüm" {...editForm.register('department')} />
            <Input label="Telefon" {...editForm.register('phone')} />
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="secondary" type="button" onClick={() => setEditingStudent(null)}>İptal</Button>
              <Button type="submit">Güncelle</Button>
            </div>
          </form>
        </Modal>
      )}

      {/* HumanInTheLoop modal */}
      {humanLoopResult?.needsHumanVerification && (
        <HumanInTheLoopModal
          items={humanLoopResult.ambiguousItems}
          onConfirm={() => setHumanLoopResult(null)}
          onCancel={() => setHumanLoopResult(null)}
        />
      )}
    </div>
  );
}

const MONTHS = ['', 'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

const paymentSchema = z.object({
  periodYear: z.coerce.number().int().min(2000).max(2100),
  periodMonth: z.coerce.number().int().min(1).max(12),
  amount: z.coerce.number().positive('Tutar 0\'dan büyük olmalıdır'),
  paymentDate: z.string().optional(),
});
type PaymentFormData = z.infer<typeof paymentSchema>;

const gradeSchema = z.object({
  courseName: z.string().min(1, 'Ders adı zorunludur'),
  exam1Grade: z.coerce.number().min(0).max(100).optional().or(z.literal('')),
  exam2Grade: z.coerce.number().min(0).max(100).optional().or(z.literal('')),
});
type GradeFormData = z.infer<typeof gradeSchema>;

function DetailPanel({
  student,
  tab,
  onTabChange,
  onHumanLoop: _onHumanLoop,
}: {
  student: StudentDto;
  tab: DetailTab;
  onTabChange: (t: DetailTab) => void;
  onHumanLoop: (r: UpsertPaymentResult | UpsertGradeResult) => void;
}) {
  const { payments, loading: pLoading, fetchPayments, upsert: upsertPayment } = usePayments(student.id);
  const { grades, loading: gLoading, fetchGrades, upsert: upsertGrade } = useExamGrades(student.id);

  const [editingPayment, setEditingPayment] = useState<InternshipPaymentDto | null>(null);
  const [showAddPayment, setShowAddPayment] = useState(false);
  const paymentForm = useForm<PaymentFormData>({ resolver: zodResolver(paymentSchema) as any });

  const [editingGrade, setEditingGrade] = useState<ExamGradeDto | null>(null);
  const [showAddGrade, setShowAddGrade] = useState(false);
  const gradeForm = useForm<GradeFormData>({ resolver: zodResolver(gradeSchema) as any });

  useEffect(() => { if (tab === 'payments') fetchPayments(); }, [tab, fetchPayments]);
  useEffect(() => { if (tab === 'exams') fetchGrades(); }, [tab, fetchGrades]);

  const openEditPayment = (payment: InternshipPaymentDto) => {
    setEditingPayment(payment);
    setShowAddPayment(false);
    paymentForm.reset({
      periodYear: payment.periodYear,
      periodMonth: payment.periodMonth,
      amount: payment.amount,
      paymentDate: payment.paymentDate ?? '',
    });
  };

  const openAddPayment = () => {
    setEditingPayment(null);
    setShowAddPayment(true);
    paymentForm.reset({
      periodYear: new Date().getFullYear(),
      periodMonth: new Date().getMonth() + 1,
      amount: 0,
      paymentDate: '',
    });
  };

  const closePaymentModal = () => { setEditingPayment(null); setShowAddPayment(false); };

  const handlePaymentSubmit = paymentForm.handleSubmit(async (data: PaymentFormData) => {
    await upsertPayment(data.periodYear, data.periodMonth, {
      amount: data.amount,
      paymentDate: data.paymentDate || undefined,
    });
    closePaymentModal();
  });

  const openEdit = (grade: ExamGradeDto) => {
    setEditingGrade(grade);
    gradeForm.reset({
      courseName: grade.courseName,
      exam1Grade: grade.exam1Grade ?? '',
      exam2Grade: grade.exam2Grade ?? '',
    });
  };

  const openAdd = () => {
    setEditingGrade(null);
    setShowAddGrade(true);
    gradeForm.reset({ courseName: '', exam1Grade: '', exam2Grade: '' });
  };

  const closeGradeModal = () => {
    setEditingGrade(null);
    setShowAddGrade(false);
  };

  const handleGradeSubmit = gradeForm.handleSubmit(async (data) => {
    await upsertGrade(data.courseName, {
      exam1Grade: data.exam1Grade === '' ? undefined : Number(data.exam1Grade),
      exam2Grade: data.exam2Grade === '' ? undefined : Number(data.exam2Grade),
    });
    closeGradeModal();
  });

  const TABS: { key: DetailTab; label: string }[] = [
    { key: 'payments', label: 'Ödemeler' },
    { key: 'exams', label: 'Sınav Notları' },
    { key: 'audit', label: 'Audit' },
  ];

  return (
    <>
      <div className="flex w-[420px] flex-col overflow-hidden bg-white">
        <div className="border-b border-gray-200 px-4 py-4">
          <p className="text-base font-semibold text-gray-900">
            {student.firstName} {student.lastName}
          </p>
          <p className="text-xs text-gray-500">{student.studentNumber} · {student.department}</p>
        </div>
        <div className="flex border-b border-gray-200">
          {TABS.map(({ key, label }) => (
            <button
              key={key}
              className={`flex-1 py-2.5 text-xs font-medium transition-colors ${
                tab === key
                  ? 'border-b-2 border-indigo-600 text-indigo-600'
                  : 'text-gray-500 hover:text-gray-700'
              }`}
              onClick={() => onTabChange(key)}
            >
              {label}
            </button>
          ))}
        </div>
        <div className="flex-1 overflow-y-auto p-4">
          {tab === 'payments' && (
            <>
              <div className="mb-3 flex justify-end">
                <Button size="sm" onClick={openAddPayment}>+ Yeni Ödeme</Button>
              </div>
              {pLoading ? <Spinner /> : <PaymentTable payments={payments} onEdit={openEditPayment} />}
            </>
          )}
          {tab === 'exams' && (
            <>
              <div className="mb-3 flex justify-end">
                <Button size="sm" onClick={openAdd}>+ Yeni Not</Button>
              </div>
              {gLoading ? <Spinner /> : <ExamGradeTable grades={grades} onEdit={openEdit} />}
            </>
          )}
          {tab === 'audit' && <AuditLogPanel studentId={student.id} />}
        </div>
      </div>

      {/* Ödeme ekle / düzenle modal */}
      {(editingPayment !== null || showAddPayment) && (
        <Modal
          title={editingPayment ? 'Ödeme Düzenle' : 'Yeni Ödeme'}
          onClose={closePaymentModal}
        >
          <form onSubmit={handlePaymentSubmit} className="flex flex-col gap-3">
            <div className="flex gap-2">
              <Input
                label="Yıl"
                type="number"
                className={editingPayment ? 'bg-gray-100 cursor-not-allowed' : ''}
                readOnly={!!editingPayment}
                {...paymentForm.register('periodYear')}
              />
              <div className="flex flex-1 flex-col gap-1">
                <label className="text-xs font-medium text-gray-700">Ay</label>
                <select
                  {...paymentForm.register('periodMonth')}
                  disabled={!!editingPayment}
                  className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-indigo-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
                >
                  {MONTHS.slice(1).map((name, i) => (
                    <option key={i + 1} value={i + 1}>{name}</option>
                  ))}
                </select>
              </div>
            </div>
            <Input
              label="Tutar (₺)"
              type="number"
              step="0.01"
              {...paymentForm.register('amount')}
              error={paymentForm.formState.errors.amount?.message}
            />
            <Input
              label="Ödeme Tarihi (opsiyonel)"
              type="date"
              {...paymentForm.register('paymentDate')}
            />
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="secondary" type="button" onClick={closePaymentModal}>İptal</Button>
              <Button type="submit">Kaydet</Button>
            </div>
          </form>
        </Modal>
      )}

      {/* Sınav notu ekle / düzenle modal */}
      {(editingGrade !== null || showAddGrade) && (
        <Modal
          title={editingGrade ? 'Sınav Notunu Düzenle' : 'Yeni Sınav Notu'}
          onClose={closeGradeModal}
        >
          <form onSubmit={handleGradeSubmit} className="flex flex-col gap-3">
            <Input
              label="Ders Adı"
              {...gradeForm.register('courseName')}
              readOnly={!!editingGrade}
              className={editingGrade ? 'bg-gray-100 cursor-not-allowed' : ''}
              error={gradeForm.formState.errors.courseName?.message}
            />
            <Input
              label="1. Sınav (0-100)"
              type="number"
              min={0}
              max={100}
              {...gradeForm.register('exam1Grade')}
              error={gradeForm.formState.errors.exam1Grade?.message}
            />
            <Input
              label="2. Sınav (0-100)"
              type="number"
              min={0}
              max={100}
              {...gradeForm.register('exam2Grade')}
              error={gradeForm.formState.errors.exam2Grade?.message}
            />
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="secondary" type="button" onClick={closeGradeModal}>İptal</Button>
              <Button type="submit">Kaydet</Button>
            </div>
          </form>
        </Modal>
      )}
    </>
  );
}

function Modal({ title, children, onClose }: { title: string; children: React.ReactNode; onClose: () => void }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-base font-semibold text-gray-900">{title}</h2>
          <button className="text-gray-400 hover:text-gray-600" onClick={onClose}>✕</button>
        </div>
        {children}
      </div>
    </div>
  );
}
