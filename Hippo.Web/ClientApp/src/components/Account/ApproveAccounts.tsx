import { useEffect, useState, useMemo, useCallback } from "react";
import { AccountModel, IRouteParams } from "../../types";
import { RejectRequest } from "../../Shared/RejectRequest";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { useParams } from "react-router-dom";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";

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

  const handleApprove = useCallback(
    async (account: AccountModel) => {
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
    },
    [accounts, cluster, setNotification]
  );

  const handleReject = useCallback(
    async (account: AccountModel) => {
      // remove the account from the list
      setAccounts(accounts?.filter((a) => a.id !== account.id));
    },
    [accounts]
  );

  const columns: Column<AccountModel>[] = useMemo(
    () => [
      {
        Header: "Groups",
        accessor: (account) => account.groups.join(", "),
        sortable: true,
      },
      {
        Header: "Name",
        accessor: (account) => account.name,
        sortable: true,
      },
      {
        Header: "Submitted",
        accessor: (account) => new Date(account.updatedOn).toLocaleDateString(),
        sortable: true,
      },
      {
        Header: "Action",
        sortable: false,
        Cell: (props) => (
          <>
            <button
              disabled={notification.pending}
              onClick={() => handleApprove(props.row.original)}
              className="btn btn-primary"
            >
              {accountApproving === props.row.original.id
                ? "Approving..."
                : "Approve"}
            </button>
            {" | "}
            {accountApproving !== props.row.original.id && (
              <RejectRequest
                account={props.row.original}
                removeAccount={() => handleReject(props.row.original)}
                updateUrl={`/api/${cluster}/Account/Reject/`}
                disabled={notification.pending}
              ></RejectRequest>
            )}
          </>
        ),
      },
    ],
    [
      accountApproving,
      cluster,
      notification.pending,
      handleApprove,
      handleReject,
    ]
  );

  const accountsData = useMemo(() => accounts ?? [], [accounts]);

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
          <ReactTable
            columns={columns}
            data={accountsData}
            initialState={{
              sortBy: [{ id: "name" }],
            }}
          />
        </div>
      </div>
    );
  }
};
