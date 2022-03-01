import { useEffect, useState } from "react";

import { Account } from "../types";
import { RejectRequest } from "../Shared/RejectRequest";

import { authenticatedFetch } from "../util/api";

export const ApproveAccounts = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account

  const [accounts, setAccounts] = useState<Account[]>();
  const [accountApproving, setAccountApproving] = useState<number>();
  const [accountRejecting, setAccountRejecting] = useState<number>();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch("/api/account/pending");

      if (response.ok) {
        setAccounts(await response.json());
      }
    };

    fetchAccounts();
  }, []);

  const handleApprove = async (account: Account) => {
    setAccountApproving(account.id);

    const response = await authenticatedFetch(
      `/api/account/approve/${account.id}`,
      { method: "POST" }
    );

    if (response.ok) {
      setAccountApproving(undefined);

      // remove the account from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
  };

  const handleReject = async (account: Account) => {
    setAccountRejecting(account.id);

    const response = await authenticatedFetch(
      `/api/account/reject/${account.id}`,
      { method: "POST" }
    );

    if (response.ok) {
      setAccountRejecting(undefined);

      // remove the account from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    }
  };

  const handleReject1 = async (account: Account) => {
    // remove the account from the list
    setAccounts(accounts?.filter((a) => a.id !== account.id));
  };

  if (accounts === undefined) {
    return <div>Loading...</div>;
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-6">
          <p>There are {accounts.length} account(s) awaiting your approval</p>
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Submitted</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account) => (
                <tr key={account.id}>
                  <td>{account.name}</td>
                  <td>{new Date(account.createdOn).toLocaleDateString()}</td>
                  <td>
                    <button
                      disabled={
                        accountApproving !== undefined &&
                        accountRejecting !== undefined
                      }
                      onClick={() => handleApprove(account)}
                      className="btn btn-primary"
                    >
                      {accountApproving === account.id
                        ? "Approving..."
                        : "Approve"}
                    </button>{" "}
                    <RejectRequest
                      account={account}
                      removeAccount={() => handleReject1(account)}
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
