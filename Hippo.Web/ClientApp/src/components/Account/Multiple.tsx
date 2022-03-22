import { useContext } from "react";
import AppContext from "../../Shared/AppContext";

// Handle redirection for people with multiple accounts
export const Multiple = () => {
  const [{ accounts }] = useContext(AppContext);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <p>
          TODO TODO: You have multiple accounts, this will be implemented soon.  For now, please use /caesfarm
        </p>
        <ul>
          {accounts.map((account) => (
            <li key={account.cluster}>{account.cluster}</li>
          ))}
        </ul>
      </div>
    </div>
  );
};
