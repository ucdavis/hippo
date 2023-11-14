import { useEffect, useState, useMemo } from "react";
import { useParams } from "react-router-dom";
import { AccountModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { getGroupModelString } from "../../util/StringHelpers";

export const ActiveAccounts = () => {
  const [accounts, setAccounts] = useState<AccountModel[]>();

  const { cluster } = useParams<IRouteParams>();

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
        Header: "Approved On",
        accessor: (row) => new Date(row.updatedOn).toLocaleDateString(),
        sortable: true,
      },
    ],
    []
  );

  const accountsData = useMemo(() => accounts ?? [], [accounts]);

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/account/active`
      );

      if (response.ok) {
        setAccounts(await response.json());
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
        .map((g) => g.name)
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
              sortBy: [{ id: "name" }],
            }}
          />
        </div>
      </div>
    );
  }
};
