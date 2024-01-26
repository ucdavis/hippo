import { useEffect, useState, useMemo, useCallback } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { User } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";

export const ClusterAdmins = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [users, setUsers] = useState<User[]>();
  const [adminRemoving, setAdminRemoving] = useState<number>();
  const [request, setRequest] = useState({
    id: "",
  });
  const { cluster } = useParams();
  const [notification, setNotification] = usePromiseNotification();

  const [getConfirmation] = useConfirmationDialog<string>(
    {
      title: "Remove Admin",
      message: "Are you sure you want to remove this administrator?",
    },
    []
  );

  useEffect(() => {
    const fetchClusterAdmins = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/admin/ClusterAdmins`
      );

      if (response.ok) {
        setUsers(await response.json());
      }
    };

    fetchClusterAdmins();
  }, [cluster]);

  const handleRemove = useCallback(
    async (user: User) => {
      const [confirmed] = await getConfirmation();
      if (!confirmed) {
        return;
      }

      setAdminRemoving(user.id);

      const req = authenticatedFetch(
        `/api/${cluster}/admin/RemoveClusterAdmin/${user.id}`,
        {
          method: "POST",
        }
      );

      setNotification(req, "Removing", "Admin Removed", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await req;

      if (response.ok) {
        // remove the user from the list
        setUsers(users?.filter((a) => a.id !== user.id));
      }
      setAdminRemoving(undefined);
    },
    [cluster, getConfirmation, setNotification, users]
  );

  const handleSubmit = async () => {
    const req = authenticatedFetch(
      `/api/${cluster}/admin/AddClusterAdmin/${request.id}`,
      {
        method: "POST",
      }
    );

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
      const newAccount = await response.json();
      //Add the user to the list
      setUsers((r) => (r ? [...r, newAccount] : [newAccount]));
      setRequest((r) => ({ ...r, id: "" }));
    }
  };

  const columns: Column<User>[] = useMemo(
    () => [
      {
        Header: "Name",
        accessor: (user) => user.name,
        sortable: true,
      },
      {
        Header: "Email",
        accessor: (user) => user.email,
        sortable: true,
      },
      {
        Header: "Action",
        sortable: false,
        Cell: (props) => (
          <>
            <button
              disabled={notification.pending}
              onClick={() => handleRemove(props.row.original)}
              className="btn btn-danger"
            >
              {adminRemoving === props.row.original.id
                ? "Removing..."
                : "Remove"}
            </button>
          </>
        ),
      },
    ],
    [adminRemoving, handleRemove, notification.pending]
  );

  const usersData = useMemo(() => users ?? [], [users]);

  if (users === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
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
          <button
            disabled={notification.pending}
            className="btn btn-primary"
            onClick={handleSubmit}
          >
            Add Admin
          </button>
          <hr />

          <p>There are {users.length} users with admin access</p>
          <ReactTable
            columns={columns}
            data={usersData}
            initialState={{
              sortBy: [{ id: "Name" }],
            }}
          />
        </div>
      </div>
    );
  }
};
