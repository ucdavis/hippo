import { useEffect, useState, useMemo, useCallback } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { User } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { HipTable } from "../../Shared/Table/HipTable";
import { createColumnHelper } from "@tanstack/react-table";
import HipButton from "../../Shared/HipComponents/HipButton";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipBody from "../../Shared/Layout/HipBody";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";

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
    [],
  );

  useEffect(() => {
    const fetchClusterAdmins = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/admin/ClusterAdmins`,
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
        },
      );

      setNotification(req, "Removing", "Admin Removed", async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
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
    [cluster, getConfirmation, setNotification, users],
  );

  const handleSubmit = async () => {
    const req = authenticatedFetch(
      `/api/${cluster}/admin/AddClusterAdmin/${request.id}`,
      {
        method: "POST",
      },
    );

    setNotification(req, "Saving", "Admin Added", async (r) => {
      if (r.status === 400) {
        const errors = await parseBadRequest(response);
        return errors;
      } else {
        return "An error happened, please try again.";
      }
    });
    const response = await req;

    if (response.ok) {
      const user = (await response.json()) as User;
      //Add the user to the list
      setUsers((r) => (r ? [...r, user] : [user]));
      setRequest((r) => ({ ...r, id: "" }));
    }
  };

  const columnHelper = createColumnHelper<User>();

  const columns = [
    columnHelper.accessor("name", {
      header: "Name",
      id: "Name", // id required for only this column for some reason
    }),
    columnHelper.accessor("email", {
      header: "Email",
    }),
    columnHelper.display({
      id: "actions",
      header: "Action",
      cell: (props) => (
        <HipButton
          color="danger"
          disabled={notification.pending}
          onClick={() => handleRemove(props.row.original)}
        >
          {adminRemoving === props.row.original.id ? "Removing..." : "Remove"}
        </HipButton>
      ),
    }),
  ];

  const usersData = useMemo(() => users ?? [], [users]);

  if (users === undefined) {
    return (
      <HipMainWrapper>
        <HipTitle title="Cluster Admins" subtitle="Admin" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        <HipTitle title="Cluster Admins" subtitle="Admin" />
        <HipBody>
          <p>There are {users.length} users with admin access</p>
          <hr />
          <h3>Add Admin</h3>
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
          <HipButton disabled={notification.pending} onClick={handleSubmit}>
            Add Admin
          </HipButton>
          <br />
          <HipTable
            columns={columns}
            data={usersData}
            initialState={{
              sorting: [{ id: "Name", desc: false }],
            }}
          />
        </HipBody>
      </HipMainWrapper>
    );
  }
};
