import { useEffect, useState } from "react";
import { Account } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { RejectRequest } from "../../Shared/RejectRequest";
import { usePromiseNotification } from "../../util/Notifications";

export const AdminApproveAccounts = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [accounts, setAccounts] = useState<Account[]>();
  const [accountApproving, setAccountApproving] = useState<number>();
  const [notification, setNotification] = usePromiseNotification();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch("/api/admin/pending");

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAccounts();
  }, []);

  const handleApprove = async (account: Account) => {
    setAccountApproving(account.id);

    const req = authenticatedFetch(`/api/admin/approve/${account.id}`, {
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
        <div className="col-md-6">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <p>There are {accounts.length} account(s) awaiting approval</p>
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
                    </button>{" "}
                    <RejectRequest
                      account={account}
                      removeAccount={() => handleReject(account)}
                      updateUrl={"/api/admin/reject/"}
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
