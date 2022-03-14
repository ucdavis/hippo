import { useEffect, useState } from "react";
import { User } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";

export const AdminUsers = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [users, setUsers] = useState<User[]>();
  const [adminRemoving, setAdminRemoving] = useState<number>();
  const [request, setRequest] = useState({
    id: "",
  });
  const [notification, setNotification] = usePromiseNotification();

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

      // remove the user from the list
      setUsers(users?.filter((a) => a.id !== user.id));
    }
    //todo deal with error
  };

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/admin/create/${request.id}`, {
      method: "POST",
    });

    setNotification(req, "Saving", "Admin Added", async (r) => {
      if (r.status === 400) {
        const errorText = await response.text(); //Bad Request Text
        return errorText;
      } else {
        return "An error happened, please try again.";
      }
    });
    const response = await req;

    if (response.ok) {
      const newUser = await response.json();
      //Add the user to the list
      setUsers((r) => (r ? [...r, newUser] : [newUser]));
      setRequest((r) => ({ ...r, id: "" }));
    }
  };

  if (users === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="form-group">
            <label className="form-label">Email or Kerberos</label>

            <input
              className="form-control"
              id="emailLookup"
              placeholder="example@ucdavis.edu"
              value={request.id}
              onChange={(e) =>
                setRequest((r) => ({ ...r, id: e.target.value }))
              }
            ></input>
          </div>
          <br />
          <button className="btn btn-primary" onClick={handleSubmit}>
            Add Admin
          </button>
          <hr />

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
