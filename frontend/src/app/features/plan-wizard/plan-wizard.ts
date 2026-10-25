import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ArrowRight, CircleAlert, LucideAngularModule } from 'lucide-angular';

import { EquipmentService } from '../../core/services/equipment.service';
import { Logo } from '../../shared/components/logo/logo';
import { DaysStep } from './steps/days-step';
import { GoalStep } from './steps/goal-step';
import { LocationStep } from './steps/location-step';
import { ReviewStep } from './steps/review-step';
import { SessionStep } from './steps/session-step';
import { WhyPanel } from './why-panel';
import { STEPS } from './wizard-options';
import { WizardStateService } from './wizard-state.service';
import { WizardStepper } from './wizard-stepper';

@Component({
  selector: 'app-plan-wizard',
  imports: [
    LucideAngularModule,
    Logo,
    WizardStepper,
    WhyPanel,
    GoalStep,
    LocationStep,
    DaysStep,
    SessionStep,
    ReviewStep,
  ],
  providers: [WizardStateService],
  templateUrl: './plan-wizard.html',
  styleUrl: './plan-wizard.css',
})
export class PlanWizard {
  protected readonly state = inject(WizardStateService);

  private readonly equipmentService = inject(EquipmentService);
  protected readonly equipment = rxResource({ stream: () => this.equipmentService.getAll() });
  // value() throws while the resource is in the error state.
  protected readonly equipmentList = computed(() =>
    this.equipment.hasValue() ? this.equipment.value() : [],
  );

  protected readonly stepCopy = computed(() => STEPS[this.state.step() - 1]);

  protected readonly ArrowRight = ArrowRight;
  protected readonly CircleAlert = CircleAlert;
}
