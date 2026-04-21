import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { Layout } from '@/components/Layout';
import { ChatPage } from '@/pages/ChatPage';
import { StudentsPage } from '@/pages/StudentsPage';
import { PaymentsPage } from '@/pages/PaymentsPage';
import { ExamsPage } from '@/pages/ExamsPage';
import { DocumentsPage } from '@/pages/DocumentsPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<ChatPage />} />
          <Route path="/students" element={<StudentsPage />} />
          <Route path="/students/:studentId/payments" element={<PaymentsPage />} />
          <Route path="/students/:studentId/exams" element={<ExamsPage />} />
          <Route path="/documents" element={<DocumentsPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
