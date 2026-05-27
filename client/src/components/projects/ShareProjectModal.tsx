import { useState, useEffect } from "react";
import type { ProjectMemberResponse } from "../../types";
import api from "../../services/api";
import Modal from "../ui/Modal";
import Button from "../ui/Button";

interface ShareProjectModalProps {
  isOpen: boolean;
  onClose: () => void;
  projectId: string;
}

interface UserSearchResult {
  id: string;
  name: string;
  email: string;
}

export default function ShareProjectModal({
  isOpen,
  onClose,
  projectId,
}: ShareProjectModalProps) {
  const [search, setSearch] = useState("");
  const [searchResults, setSearchResults] = useState<UserSearchResult[]>([]);
  const [members, setMembers] = useState<ProjectMemberResponse[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [isAdding, setIsAdding] = useState<string | null>(null);
  const [isRemoving, setIsRemoving] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) fetchMembers();
  }, [isOpen]);

  useEffect(() => {
    if (search.trim().length < 2) {
      setSearchResults([]);
      return;
    }
    const timeout = setTimeout(() => searchUsers(), 300);
    return () => clearTimeout(timeout);
  }, [search]);

  const fetchMembers = async () => {
    try {
      const { data } = await api.get(`/api/projects/${projectId}/members`);
      setMembers(data);
    } catch {
      console.error("Failed to fetch members");
    }
  };

  const searchUsers = async () => {
    setIsSearching(true);
    try {
      const { data } = await api.get(`/api/users/search?name=${search}`);
      const memberIds = members.map((m) => m.userId);
      setSearchResults(data.filter((u: UserSearchResult) => !memberIds.includes(u.id)));
    } catch {
      console.error("Failed to search users");
    } finally {
      setIsSearching(false);
    }
  };

  const handleAddMember = async (userId: string) => {
    setIsAdding(userId);
    try {
      await api.post(`/api/projects/${projectId}/members`, { userId });
      await fetchMembers();
      setSearchResults((prev) => prev.filter((u) => u.id !== userId));
      setSearch("");
    } catch {
      console.error("Failed to add member");
    } finally {
      setIsAdding(null);
    }
  };

  const handleRemoveMember = async (userId: string) => {
    setIsRemoving(userId);
    try {
      await api.delete(`/api/projects/${projectId}/members/${userId}`);
      setMembers((prev) => prev.filter((m) => m.userId !== userId));
    } catch {
      console.error("Failed to remove member");
    } finally {
      setIsRemoving(null);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Share Project">
      <div className="space-y-6">

        {/* Search */}
        <div>
          <label className="text-sm font-medium text-brand-text block mb-2">
            Add people by name
          </label>
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search by name..."
            className="w-full px-4 py-2.5 rounded-xl border border-brand-border bg-brand-bg text-brand-text placeholder-brand-text-light focus:outline-none focus:ring-2 focus:ring-brand-primary text-sm"
          />

          {/* Search results */}
          {isSearching && (
            <p className="text-xs text-brand-text-light mt-2">Searching...</p>
          )}
          {searchResults.length > 0 && (
            <div className="mt-2 border border-brand-border rounded-xl overflow-hidden">
              {searchResults.map((user) => (
                <div
                  key={user.id}
                  className="flex items-center justify-between px-4 py-3 hover:bg-brand-bg border-b border-brand-border last:border-0"
                >
                  <div>
                    <p className="text-sm font-medium text-brand-text">{user.name}</p>
                    <p className="text-xs text-brand-text-light">{user.email}</p>
                  </div>
                  <Button
                    size="sm"
                    onClick={() => handleAddMember(user.id)}
                    disabled={isAdding === user.id}
                  >
                    {isAdding === user.id ? "Adding..." : "Add"}
                  </Button>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Current members */}
        <div>
          <label className="text-sm font-medium text-brand-text block mb-2">
            Current members
          </label>
          {members.length === 0 ? (
            <p className="text-sm text-brand-text-light">
              No members yet — share this project with someone!
            </p>
          ) : (
            <div className="border border-brand-border rounded-xl overflow-hidden">
              {members.map((member) => (
                <div
                  key={member.userId}
                  className="flex items-center justify-between px-4 py-3 hover:bg-brand-bg border-b border-brand-border last:border-0"
                >
                  <div>
                    <p className="text-sm font-medium text-brand-text">{member.name}</p>
                    <p className="text-xs text-brand-text-light">{member.email}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-brand-text-light bg-brand-bg px-2 py-1 rounded-full border border-brand-border">
                      {member.role}
                    </span>
                    <button
                      onClick={() => handleRemoveMember(member.userId)}
                      disabled={isRemoving === member.userId}
                      className="text-xs text-brand-error hover:opacity-75 transition-all"
                    >
                      {isRemoving === member.userId ? "Removing..." : "Remove"}
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </Modal>
  );
}
