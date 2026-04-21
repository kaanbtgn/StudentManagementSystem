import { useState, useCallback } from 'react';
import { studentApi } from '@/api/studentApi';
import { useStudentStore } from '@/store/studentStore';
import type { CreateStudentRequest, UpdateStudentRequest } from '@/types/student.types';
import { useShallow } from 'zustand/react/shallow';

export function useStudents() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { students, setStudents, upsertStudent, removeStudent } = useStudentStore(
    useShallow((s) => ({
      students: s.students,
      setStudents: s.setStudents,
      upsertStudent: s.upsertStudent,
      removeStudent: s.removeStudent,
    }))
  );

  const fetchAll = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await studentApi.getAll();
      setStudents(data);
    } catch {
      setError('Öğrenciler yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, [setStudents]);

  const search = useCallback(
    async (term: string) => {
      setLoading(true);
      setError(null);
      try {
        const data = await studentApi.search(term);
        setStudents(data);
      } catch {
        setError('Arama başarısız.');
      } finally {
        setLoading(false);
      }
    },
    [setStudents]
  );

  const create = useCallback(
    async (payload: CreateStudentRequest) => {
      const student = await studentApi.create(payload);
      upsertStudent(student);
      return student;
    },
    [upsertStudent]
  );

  const update = useCallback(
    async (id: string, payload: UpdateStudentRequest) => {
      // Backend 204 No Content döndürüyor — store'daki mevcut kaydı payload ile birleştiriyoruz
      await studentApi.update(id, payload);
      const existing = students.find((s) => s.id === id);
      if (existing) {
        upsertStudent({ ...existing, ...payload });
      }
    },
    [students, upsertStudent]
  );

  const remove = useCallback(
    async (id: string) => {
      await studentApi.remove(id);
      removeStudent(id);
    },
    [removeStudent]
  );

  return { loading, error, fetchAll, search, create, update, remove };
}
