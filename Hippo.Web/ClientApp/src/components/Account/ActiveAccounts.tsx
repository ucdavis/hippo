import { useEffect, useState, useMemo, useCallback } from "react";
import { useParams } from "react-router-dom";
import { AccountModel, RawAccountModel } from "../../types";
import { authenticatedFetch, parseRawAccountModel } from "../../util/api";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { getGroupModelString } from "../../util/StringHelpers";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import ObjectTree from "../../Shared/ObjectTree";

export const ActiveAccounts = () => {
  const [accounts, setAccounts] = useState<AccountModel[]>();
  const [viewing, setViewing] = useState<number>();

  const { cluster } = useParams();

  const [showDetails] = useConfirmationDialog(
    {
      title: "Account Details",
      message: () => {
        const account = accounts.find((a) => a.id === viewing);
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <label className="form-label">Name</label>
                <input
                  className="form-control"
                  id="accountDetailsName"
                  value={account.name}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Email</label>
                <input
                  className="form-control"
                  id="accountDetailsEmail"
                  value={account.email}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Kerberos</label>
                <input
                  className="form-control"
                  id="accountDetailsKerberos"
                  value={account.kerberos}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Updated On</label>
                <input
                  className="form-control"
                  id="accountDetailsUpdatedOn"
                  value={new Date(account.updatedOn).toLocaleDateString()}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Details</label>
                <ObjectTree obj={account.data} />
              </div>
            </div>
          </div>
        );
      },
      buttons: ["OK"],
    },
    [accounts, viewing],
  );

  const handleDetails = useCallback(
    async (account: AccountModel) => {
      setViewing(account.id);
      await showDetails();
      setViewing(undefined);
    },
    [showDetails],
  );

  const columns: Column<AccountModel>[] = useMemo(
    () => [
      {
        Header: "Groups",
        accessor: (row) => getGroupModelString(row.memberOfGroups),
        Cell: (props) => (
          <>
            {props.row.original.memberOfGroups.map((g, i) => (
              <>
                {i > 0 && ", "}
                <GroupNameWithTooltip
                  group={g}
                  id={props.row.original.id.toString()}
                  key={i}
                />
              </>
            ))}
          </>
        ),
        sortable: true,
      },
      {
        Header: "Name",
        accessor: (row) => row.name,
        sortable: true,
      },
      {
        Header: "Email",
        accessor: (row) => row.email,
        sortable: true,
      },
      {
        Header: "Kerberos",
        accessor: (row) => row.kerberos,
        sortable: true,
      },
      {
        Header: "Updated On",
        accessor: (row) => new Date(row.updatedOn).toLocaleDateString(),
        sortable: true,
      },
      {
        Header: "Action",
        Cell: (props) => (
          <>
            <button
              onClick={() => handleDetails(props.row.original)}
              className="btn btn-primary"
            >
              {viewing === props.row.original.id ? "Viewing..." : "Details"}
            </button>
          </>
        ),
      },
    ],
    [handleDetails, viewing],
  );

  const accountsData = useMemo(() => accounts ?? [], [accounts]);

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/account/active`,
      );

      if (response.ok) {
        setAccounts(
          ((await response.json()) as RawAccountModel[]).map(
            parseRawAccountModel,
          ),
        );
      }
    };

    fetchAccounts();
  }, [cluster]);

  if (accounts === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    const groupCount = new Set(
      accounts
        .map((a) => a.memberOfGroups)
        .flat()
        .filter((g) => g !== null)
        .map((g) => g.name),
    ).size;
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
          <p>
            You have {accounts.length} active account(s) in {groupCount}{" "}
            group(s)
          </p>
          <ReactTable
            columns={columns}
            data={accountsData}
            initialState={{
              sortBy: [{ id: "Groups" }, { id: "Name" }],
            }}
          />
        </div>
      </div>
    );
  }
};
