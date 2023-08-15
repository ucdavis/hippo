import { useEffect, useState } from "react";
import { AccountModel, IRouteParams } from "../../types";
import { RejectRequest } from "../../Shared/RejectRequest";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { useParams } from "react-router-dom";

export const ApproveAccounts = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [accounts, setAccounts] = useState<AccountModel[]>();
  const [accountApproving, setAccountApproving] = useState<number>();
  const [notification, setNotification] = usePromiseNotification();

  const { cluster } = useParams<IRouteParams>();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/account/pending`
      );

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAccounts();
  }, [cluster]);

  const handleApprove = async (account: AccountModel) => {
    setAccountApproving(account.id);

    const req = authenticatedFetch(
      `/api/${cluster}/account/approve/${account.id}`,
      {
        method: "POST",
      }
    );

    setNotification(req, "Approving", "Account Approved");

    const response = await req;
    if (response.ok) {
      setAccountApproving(undefined);

      // remove the account from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
  };

  const handleReject = async (account: AccountModel) => {
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
          <p>There are {accounts.length} account(s) awaiting approval</p>
          <table className="table">
            <thead>
              <tr>
                <th>Group</th>
                <th>Name</th>
                <th>Submitted</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.group}</td>
                  <td>{account.name}</td>
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
                    </button>
                    {" | "}
                    {accountApproving !== account.id && (
                      <RejectRequest
                        account={account}
                        removeAccount={() => handleReject(account)}
                        updateUrl={`/api/${cluster}/Account/Reject/`}
                        disabled={notification.pending}
                      ></RejectRequest>
                    )}
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
