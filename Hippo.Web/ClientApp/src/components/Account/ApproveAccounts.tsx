import { useEffect, useState } from "react";
import { AccountModel, IRouteParams } from "../../types";
import { RejectRequest } from "../../Shared/RejectRequest";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { useParams } from "react-router-dom";
import { DataTable } from "../../Shared/DataTable";

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

    setNotification(
      req,
      "Approving",
      "Account Approved. Please allow 2 to 3 hours for changes to take place."
    );

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
          <DataTable
            keyField="id"
            data={accounts}
            responsive
            columns={[
              {
                name: <th>Groups</th>,
                selector: (account) => account.groups.join(", "),
                sortable: true,
              },
              {
                name: <th>Name</th>,
                selector: (account) => account.name,
                sortable: true,
              },
              {
                name: <th>Submitted</th>,
                selector: (account) =>
                  new Date(account.updatedOn).toLocaleDateString(),
                sortable: true,
              },
              {
                name: <th>Action</th>,
                sortable: false,
                cell: (account) => (
                  <>
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
                  </>
                ),
              },
            ]}
          />
        </div>
      </div>
    );
  }
};
