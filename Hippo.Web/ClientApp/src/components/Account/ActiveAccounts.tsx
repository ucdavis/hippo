import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { AccountModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import DataTable from "../../Shared/DataTableBase";

export const ActiveAccounts = () => {
  const [accounts, setAccounts] = useState<AccountModel[]>();

  const { cluster } = useParams<IRouteParams>();

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
        .map((a) => a.groups)
        .flat()
        .filter((g) => g !== null)
    ).size;
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
          <p>
            You have {accounts.length} active account(s) in {groupCount}{" "}
            group(s)
          </p>
          <DataTable
            keyField="id"
            data={accounts}
            responsive
            columns={[
              {
                name: "Groups",
                selector: (row) => row.groups.join(", "),
                sortable: true,
              },
              { name: "Name", selector: (row) => row.name, sortable: true },
              {
                name: "Approved On",
                selector: (row) => new Date(row.updatedOn).toLocaleDateString(),
                sortable: true,
              },
            ]}
          />
        </div>
      </div>
    );
  }
};
