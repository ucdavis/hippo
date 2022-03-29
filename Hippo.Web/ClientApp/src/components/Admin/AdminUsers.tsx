import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { Account, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";

export const AdminUsers = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [accounts, setAccounts] = useState<Account[]>();
  const [adminRemoving, setAdminRemoving] = useState<number>();
  const [request, setRequest] = useState({
    id: "",
  });
  const { cluster } = useParams<IRouteParams>();
  const [notification, setNotification] = usePromiseNotification();

  const [getConfirmation] = useConfirmationDialog<string>(
    {
      title: "Remove Admin",
      message: "Are you sure you want to remove this administrator?",
    },
    []
  );

  useEffect(() => {
    const fetchAdminAccounts = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/admin/index`);

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAdminAccounts();
  }, [cluster]);

  const handleRemove = async (account: Account) => {
    const [confirmed] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    setAdminRemoving(account.id);

    const req = authenticatedFetch(
      `/api/${cluster}/admin/Remove/${account.owner?.id}`,
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
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
    setAdminRemoving(undefined);
  };

  const handleSubmit = async () => {
    const req = authenticatedFetch(
      `/api/${cluster}/admin/create/${request.id}`,
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
      setAccounts((r) => (r ? [...r, newAccount] : [newAccount]));
      setRequest((r) => ({ ...r, id: "" }));
    }
  };

  if (accounts === undefined) {
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

          <p>There are {accounts.length} users with admin access</p>
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.owner?.name}</td>
                  <td>{account.owner?.email}</td>
                  <td>
                    <button
                      disabled={notification.pending}
                      onClick={() => handleRemove(account)}
                      className="btn btn-primary"
                    >
                      {adminRemoving === account.id ? "Removing..." : "Remove"}
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
