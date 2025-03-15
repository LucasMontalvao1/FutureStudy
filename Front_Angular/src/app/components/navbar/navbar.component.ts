import { Component, OnInit, Input, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
<<<<<<< Updated upstream
  @Input() appName: string = 'FutureApp';
  isMenuOpen: boolean = false;
  isMobileView: boolean = false;
=======
  @Input() appName: string = 'FutureStudy';
>>>>>>> Stashed changes
  isScrolled: boolean = false;
  isMenuOpen: boolean = false;
  cadastroMenuOpen: boolean = false;
<<<<<<< Updated upstream

=======
  userData: any = null;
  isDesktopView: boolean = false;
  
>>>>>>> Stashed changes
  constructor(
    private router: Router,
    private authService: AuthService,
    private elementRef: ElementRef
  ) { }

  ngOnInit(): void {
<<<<<<< Updated upstream
    this.checkScreenSize();
=======
>>>>>>> Stashed changes
    // Obter dados do usuário do token
    if (this.authService.isAuthenticated()) {
      this.userData = this.authService.getDecodedToken();
    }
    
    // Verificar tamanho da tela
    this.checkScreenSize();
  }

  @HostListener('window:scroll', [])
  onWindowScroll(): void {
    this.isScrolled = window.scrollY > 20;
  }

  @HostListener('window:resize', [])
  onResize(): void {
    this.checkScreenSize();
  }

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    // Fechar dropdown ao clicar fora
<<<<<<< Updated upstream
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.cadastroMenuOpen = false;
    }
  }

  checkScreenSize() {
    this.isMobileView = window.innerWidth < 992;
    if (!this.isMobileView) {
      this.isMenuOpen = false;
    }
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
    
    // Bloquear o scroll do body quando o menu está aberto em dispositivos móveis
    if (this.isMenuOpen) {
      document.body.classList.add('menu-open');
      this.cadastroMenuOpen = false;
=======
    const dropdownElement = this.elementRef.nativeElement.querySelector('.dropdown');
    const sideMenuElement = this.elementRef.nativeElement.querySelector('.side-menu');
    
    if (this.isDesktopView) {
      // Em desktop, verifica apenas o clique fora do dropdown
      if (dropdownElement && !dropdownElement.contains(event.target as Node)) {
        this.cadastroMenuOpen = false;
      }
>>>>>>> Stashed changes
    } else {
      // Em mobile, mantém o menu aberto
      if (!sideMenuElement?.contains(event.target as Node)) {
        // Mas fecha o dropdown se clicar fora dele dentro do menu
        const menuDropdownElement = this.elementRef.nativeElement.querySelector('.menu-group');
        if (menuDropdownElement && !menuDropdownElement.contains(event.target as Node)) {
          this.cadastroMenuOpen = false;
        }
      }
    }
  }
<<<<<<< Updated upstream

=======
  
  checkScreenSize(): void {
    this.isDesktopView = window.innerWidth >= 1500;
    
    // Se estiver em visualização desktop, fecha o menu lateral
    if (this.isDesktopView && this.isMenuOpen) {
      this.closeMenu();
    }
  }
  
  toggleMenu(event: Event): void {
    if (!this.isDesktopView) {
      event.preventDefault();
      this.isMenuOpen = !this.isMenuOpen;
      
      // Prevenir scroll quando o menu está aberto em mobile
      if (this.isMenuOpen) {
        document.body.style.overflow = 'hidden';
      } else {
        document.body.style.overflow = '';
      }
    }
  }
  
  closeMenu(): void {
    this.isMenuOpen = false;
    document.body.style.overflow = '';
  }
  
>>>>>>> Stashed changes
  toggleCadastroMenu(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.cadastroMenuOpen = !this.cadastroMenuOpen;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}