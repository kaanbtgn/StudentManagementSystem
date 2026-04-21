import { create } from 'zustand';
import type { StudentDto } from '@/types/student.types';

interface StudentState {
  students: StudentDto[];
  selectedStudent: StudentDto | null;
  setStudents: (students: StudentDto[]) => void;
  setSelectedStudent: (student: StudentDto | null) => void;
  upsertStudent: (student: StudentDto) => void;
  removeStudent: (id: string) => void;
}

export const useStudentStore = create<StudentState>((set) => ({
  students: [],
  selectedStudent: null,

  setStudents: (students) => set({ students }),

  setSelectedStudent: (student) => set({ selectedStudent: student }),

  upsertStudent: (student) =>
    set((state) => {
      const exists = state.students.some((s) => s.id === student.id);
      return {
        students: exists
          ? state.students.map((s) => (s.id === student.id ? student : s))
          : [...state.students, student],
      };
    }),

  removeStudent: (id) =>
    set((state) => ({
      students: state.students.filter((s) => s.id !== id),
      selectedStudent: state.selectedStudent?.id === id ? null : state.selectedStudent,
    })),
}));
