import { useEffect, useState } from "react";
import { Account, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { RejectRequest } from "../../Shared/RejectRequest";
import { usePromiseNotification } from "../../util/Notifications";
import { useParams } from "react-router-dom";

export const AdminApproveAccounts = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [accounts, setAccounts] = useState<Account[]>();
  const [accountApproving, setAccountApproving] = useState<number>();
  const [notification, setNotification] = usePromiseNotification();

  const { cluster } = useParams<IRouteParams>();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/admin/pending`);

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAccounts();
  }, [cluster]);

  const handleApprove = async (account: Account) => {
    setAccountApproving(account.id);

    const req = authenticatedFetch(`/api/${cluster}/admin/approve/${account.id}`, {
      method: "POST",
    });

    setNotification(req, "Approving", "Account Approved");

    var response = await req;

    if (response.ok) {
      setAccountApproving(undefined);

      // remove the account from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
  };

  const handleReject = async (account: Account) => {
    // remove the account from the list
    setAccounts(accounts?.filter((a) => a.id !== account.id));
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
          <p>
            There are {accounts.length} account(s) awaiting sponsor approval.
            You may override the sponsor and approve or reject them.
          </p>
          <table className="table">
            <thead>
              <tr>
                <th>Requestor</th>
                <th>Sponsor</th>
                <th>Submitted</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.name}</td>
                  <td>
                    {account.sponsor?.name} ({account.sponsor?.owner?.email})
                  </td>
                  <td>{new Date(account.createdOn).toLocaleDateString()}</td>
                  <td>
                    <button
                      disabled={notification.pending}
                      onClick={() => handleApprove(account)}
                      className="btn btn-primary"
                    >
                      {accountApproving === account.id
                        ? "Approving..."
                        : "Approve"}
                    </button>{" | "}
                    <RejectRequest
                      account={account}
                      removeAccount={() => handleReject(account)}
                      updateUrl={`/api/${cluster}/admin/reject/`}
                      disabled={notification.pending}
                    ></RejectRequest>
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
