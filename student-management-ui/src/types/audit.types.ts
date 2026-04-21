export interface AuditEntry {
  id: string;
  entityId: string;
  entityType: string;
  action: string;
  timestamp: string;
  oldValues?: Record<string, unknown>;
  newValues?: Record<string, unknown>;
}
