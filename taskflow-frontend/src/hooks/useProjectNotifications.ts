import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

const getHubUrl = () => {
  const base = import.meta.env.VITE_API_URL || '';
  if (base.startsWith('http')) {
    try {
      const u = new URL(base);
      return `${u.origin}/hubs/notifications`;
    } catch {
      return `${window.location.origin}/hubs/notifications`;
    }
  }
  return `${window.location.origin}/hubs/notifications`;
};

export function useProjectNotifications(
  projectId: string | undefined,
  onTaskUpdated: (message: string, data?: { taskId: string; task?: unknown }) => void
) {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const onTaskUpdatedRef = useRef(onTaskUpdated);
  onTaskUpdatedRef.current = onTaskUpdated;

  useEffect(() => {
    if (!projectId) return;

    const token = localStorage.getItem('token');
    if (!token) return;

    const hubUrl = getHubUrl();
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    connection.on('TaskUpdated', (data: { message: string; taskId: string; task?: unknown }) => {
      onTaskUpdatedRef.current(data.message, { taskId: data.taskId, task: data.task });
    });

    connection
      .start()
      .then(() => connection.invoke('JoinProject', projectId))
      .catch((err) => console.warn('SignalR connect failed:', err));

    connectionRef.current = connection;

    return () => {
      connection.invoke('LeaveProject', projectId).catch(() => {});
      connection.stop().catch(() => {});
      connectionRef.current = null;
    };
  }, [projectId]);
}
