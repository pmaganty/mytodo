import type { Task } from "../../types";
import TaskRow from "./TaskRow";
import Button from "../ui/Button";

interface TaskListProps {
  tasks: Task[];
  onAddTask: () => void;
  onTaskUpdated: (task: Task) => void;
}

export default function TaskList({ tasks, onAddTask, onTaskUpdated }: TaskListProps) {
  return (
    <div className="bg-brand-paper border border-brand-border rounded-2xl overflow-hidden">
      <div className="flex items-center justify-between px-6 py-4 border-b border-brand-border">
        <h3 className="font-display text-lg font-bold text-brand-text">Tasks</h3>
        <Button size="sm" onClick={onAddTask}>+ Add Task</Button>
      </div>

      {tasks.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-3xl mb-3">✅</p>
          <p className="font-display text-lg font-bold text-brand-text mb-1">No tasks yet</p>
          <p className="text-brand-text-light text-sm">Add your first task to get started</p>
        </div>
      ) : (
        <table className="w-full">
          <thead>
            <tr className="border-b border-brand-border bg-brand-bg">
              <th className="py-3 px-4 w-10"></th>
              <th className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider">Task</th>
              <th className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider">Status</th>
              <th className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider">Priority</th>
              <th className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider">Due Date</th>
            </tr>
          </thead>
          <tbody>
            {tasks.map((task) => (
              <TaskRow key={task.id} task={task} onTaskUpdated={onTaskUpdated} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
