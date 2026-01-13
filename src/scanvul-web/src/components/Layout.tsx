import { useEffect } from "react";
import { Outlet, Link, useNavigate } from "react-router-dom";
import { Sun, Moon, LogOut, ShieldCheck, Languages } from "lucide-react";
import { useThemeStore } from "../store/themeStore";
import { useAuthStore } from "../store/authStore";
import { useTranslation } from "react-i18next";

export const Layout = () => {
  const { isDark, toggleTheme } = useThemeStore();
  const { logout } = useAuthStore();
  const navigate = useNavigate();
  const { t } = useTranslation();

  useEffect(() => {
    const root = window.document.documentElement;
    if (isDark) root.classList.add("dark");
    else root.classList.remove("dark");
  }, [isDark]);

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const { i18n } = useTranslation();

  const toggleLanguage = () => {
    const newLang = i18n.language === "en" ? "ru" : "en";
    i18n.changeLanguage(newLang);
  };

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-card text-card-foreground border-b border-gray-200 dark:border-gray-800 px-6 py-4 flex items-center justify-between shadow-sm">
        <Link to="/" className="hover:text-primary transition-colors">
          <div className="flex items-center gap-3">
            <ShieldCheck className="h-8 w-8 text-primary" />
            <h1 className="text-xl font-bold tracking-tight">{t("app.title")}</h1>
          </div>
        </Link>
        <div className="flex items-center gap-4">
          <button
            onClick={toggleLanguage}
            className="p-2 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors flex items-center gap-1"
            title="Switch Language"
          >
            <Languages className="h-5 w-5" />
            <span className="text-xs font-bold w-4">
              {i18n.language.toUpperCase()}
            </span>
          </button>
          <button
            onClick={toggleTheme}
            className={
              "p-2 rounded-full transition hover:bg-gray-400 dark:hover:bg-gray-700"
            }
          >
            {isDark ? (
              <Sun className="h-5 w-5" />
            ) : (
              <Moon className="h-5 w-5" />
            )}
          </button>
          <button
            onClick={handleLogout}
            className="p-2 text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-full transition"
          >
            <LogOut className="h-5 w-5" />
          </button>
        </div>
      </header>
      <main className="flex-1 container mx-auto p-6">
        <Outlet />
      </main>
    </div>
  );
};
