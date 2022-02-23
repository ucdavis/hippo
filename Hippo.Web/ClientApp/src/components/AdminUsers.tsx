import { useEffect, useState } from "react";

import { User } from "../types";

import { authenticatedFetch } from "../util/api";

export const AdminUsers = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [users, setUsers] = useState<User[]>();
  const [adminRemoving, setAdminRemoving] = useState<number>();

  useEffect(() => {
    const fetchAdminUsers = async () => {
      const response = await authenticatedFetch("/api/admin/index");

      if (response.ok) {
        setUsers(await response.json());
      }
    };

    fetchAdminUsers();
  }, []);

  const handleRemove = async (user: User) => {
    setAdminRemoving(user.id);

    const response = await authenticatedFetch(`/api/admin/Remove/${user.id}`, {
      method: "POST",
    });

    if (response.ok) {
      setAdminRemoving(undefined);

      // remove the account from the list
      setUsers(users?.filter((a) => a.id !== user.id));
    }
  };

  if (users === undefined) {
    return <div>Loading...</div>;
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <p>There are {users.length} users with admin access</p>
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.name}</td>
                  <td>{user.email}</td>
                  <td>
                    <button
                      disabled={adminRemoving !== undefined}
                      onClick={() => handleRemove(user)}
                      className="btn btn-primary"
                    >
                      {adminRemoving === user.id ? "Removing..." : "Remove"}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    );
  }
};
