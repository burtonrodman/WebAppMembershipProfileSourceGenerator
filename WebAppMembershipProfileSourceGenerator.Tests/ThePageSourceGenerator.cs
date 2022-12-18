using burtonrodman.WebAppMembershipProfileSourceGenerator;

namespace PageSourceGeneratorTests;

public class ThePageSourceGenerator : GeneratorTestBase<PageSourceGenerator>
{
    public ThePageSourceGenerator() : base(new List<string>() {
        """
        Public Class ProfileCommon
        End Class
        """,
        """
        Namespace Global.System.Web.UI
            Public Class Page
            End Class
            Public Class MasterPage
            End Class
            Public Class UserControl
            End Class
        End Namespace
        """,
        """
        Namespace Global.NotSystemWebUI
            Public Class Page
            End Class
        End Namespace
        """
    })
    { }

    [Fact]
    public void FindsPartialClassThatInheritsSystemWebUIPage()
    {
        RunTestWithDriver(
            """
            Partial Class Profile
                Inherits System.Web.UI.Page
            End Class
            """,
            new() {
                {
                    "UnitTest.Profile.g.vb",
                    """
                    Partial Class Profile
                        Property Profile As ProfileCommon
                    End Class
                    """
                }
            }
        );
    }

    [Fact]
    public void FindsPartialClassThatInheritsImportedPage()
    {
        RunTestWithDriver(
            """
            Imports System.Web.UI
            Partial Class Profile
                Inherits Page
            End Class
            """,
            new() {
                {
                    "UnitTest.Profile.g.vb",
                    """
                    Partial Class Profile
                        Property Profile As ProfileCommon
                    End Class
                    """
                }
            }
        );
    }

    [Fact]
    public void OnlyGeneratesForPartialClassesWithInheritsSyntax()
    {
        RunTestWithDriver(
            """
            Imports System.Web.UI
            Partial Class Profile
                Inherits Page
            End Class
            Partial Class Profile
            End Class
            """,
            new() {
                {
                    "UnitTest.Profile.g.vb",
                    """
                    Partial Class Profile
                        Property Profile As ProfileCommon
                    End Class
                    """
                }
            }
        );
    }

    [Fact]
    public void OnlyGeneratesOneNewClass()
    {
        RunTestWithDriver(
            """
            Imports System.Web.UI
            Partial Class Profile
                Inherits Page
            End Class
            Partial Class Profile
                Inherits Page
            End Class
            """,
            new() {
                {
                    "UnitTest.Profile.g.vb",
                    """
                    Partial Class Profile
                        Property Profile As ProfileCommon
                    End Class
                    """
                }
            }
        );
    }

    [Fact]
    public void IncludesNamespace()
    {
        RunTestWithDriver(
            """
            Imports System.Web.UI
            Partial Class Profile
                Inherits Page
            End Class
            Namespace Test
                Partial Class Profile
                    Inherits Page
                End Class
            End Namespace
            """,
            new() {
                {
                    "UnitTest.Profile.g.vb",
                    """
                    Partial Class Profile
                        Property Profile As ProfileCommon
                    End Class
                    """
                },
                {
                    "UnitTest.Test.Profile.g.vb",
                    """
                    Namespace Test
                        Partial Class Profile
                            Property Profile As ProfileCommon
                        End Class
                    End Namespace
                    """
                }
            }
        );
    }

    [Fact]
    public void IgnoresClassesThatDontInheritPage()
    {
        RunTestWithDriver(
            """
            Partial Class NotAPage
            End Class
            """,
            new() { }
        );
    }

    [Fact]
    public void IgnoresClassesThatInheritOtherNamespacePage()
    {
        RunTestWithDriver(
            """
            Imports NotSystemWebUI
            Partial Class NotAPage
                Inherits Page
            End Class
            """,
            new() { }
        );
    }

    [Fact]
    public void FindsPartialClassThatInheritsSystemWebUIMasterPage()
    {
        RunTestWithDriver(
            """
            Partial Class Profile
                Inherits System.Web.UI.MasterPage
            End Class
            """,
            new() {
                {
                    "UnitTest.Profile.g.vb",
                    """
                    Partial Class Profile
                        Property Profile As ProfileCommon
                    End Class
                    """
                }
            }
        );
    }

    [Fact]
    public void FindsPartialClassThatInheritsSystemWebUIUserControl()
    {
        RunTestWithDriver(
            """
            Partial Class Profile
                Inherits System.Web.UI.UserControl
            End Class
            """,
            new() {
                {
                    "UnitTest.Profile.g.vb",
                    """
                    Partial Class Profile
                        Property Profile As ProfileCommon
                    End Class
                    """
                }
            }
        );
    }
}
